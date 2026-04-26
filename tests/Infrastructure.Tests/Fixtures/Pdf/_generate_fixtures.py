# PDF fixture generator for PdfConversionService binary-fixture tests (RECEIPTS-651).
#
# Synthesizes small (< 100 KB) real PDFs that exercise color spaces and filters
# that PdfPig.Writer.PdfDocumentBuilder cannot emit. PDFium (via PDFtoImage in
# the production code) is expected to rasterize each of these without error.
#
# Run:
#     python tests/Infrastructure.Tests/Fixtures/Pdf/_generate_fixtures.py
#
# Requires: reportlab, Pillow. JBIG2 generation requires the external
# `jbig2enc` binary (not available on most developer machines), so the JBIG2
# fixture is intentionally absent — RECEIPTS-651 documents the gap.

import os
import sys
import zlib
from pathlib import Path

from PIL import Image
from reportlab.pdfgen import canvas
from reportlab.lib.pagesizes import letter
from reportlab.lib.colors import CMYKColor


HERE = Path(__file__).parent


def write_cmyk_pdf() -> None:
    """CMYK color space — drawn rectangles + CMYK-tagged JPEG embedded image.

    PDFium must handle the CMYK DeviceCMYK color space and the CMYK JPEG
    DCTDecode stream when rasterizing.
    """
    out = HERE / "cmyk-receipt.pdf"
    c = canvas.Canvas(str(out), pagesize=letter)

    # Page-level CMYK fills exercise the DeviceCMYK color space.
    c.setFillColor(CMYKColor(0.0, 0.7, 1.0, 0.1))  # warm orange-ish
    c.rect(72, 700, 200, 60, fill=1, stroke=0)

    c.setFillColor(CMYKColor(0.0, 0.0, 0.0, 1.0))  # black
    c.setFont("Helvetica", 14)
    c.drawString(72, 660, "WALMART SUPERCENTER")
    c.drawString(72, 640, "MILK 2% $3.49")
    c.drawString(72, 620, "TOTAL $3.74")

    # Embed a small CMYK JPEG. PIL's JPEG encoder writes Adobe-marker CMYK
    # JPEGs; PDFium's DCTDecode + CMYK color space handles these natively.
    cmyk = Image.new("CMYK", (60, 60))
    pixels = cmyk.load()
    for y in range(60):
        for x in range(60):
            pixels[x, y] = (x * 4 % 256, y * 4 % 256, (x + y) * 2 % 256, 30)
    jpeg_path = HERE / "_tmp-cmyk.jpg"
    cmyk.save(jpeg_path, format="JPEG", quality=70)
    c.drawImage(str(jpeg_path), 72, 540, width=60, height=60)
    jpeg_path.unlink()

    c.save()
    _assert_size_under(out, 100_000)


def write_indexed_pdf() -> None:
    """Indexed (palette) color space — small palette PNG embedded into a PDF."""
    out = HERE / "indexed-receipt.pdf"

    # Build a tiny 8-color palette image. reportlab will embed this via the
    # PIL backend, producing a /Indexed color-space PDF image XObject.
    palette = []
    for r, g, b in [
        (255, 0, 0), (0, 255, 0), (0, 0, 255),
        (255, 255, 0), (255, 0, 255), (0, 255, 255),
        (0, 0, 0), (255, 255, 255),
    ]:
        palette.extend([r, g, b])
    palette.extend([0] * (768 - len(palette)))  # pad to 256 entries

    img = Image.new("P", (40, 40))
    img.putpalette(palette)
    pixels = img.load()
    for y in range(40):
        for x in range(40):
            pixels[x, y] = (x + y) % 8
    png_path = HERE / "_tmp-indexed.png"
    img.save(png_path, format="PNG", optimize=True)

    c = canvas.Canvas(str(out), pagesize=letter)
    c.setFont("Helvetica", 14)
    c.drawString(72, 720, "INDEXED COLOR PDF")
    c.drawString(72, 700, "TOTAL $1.00")
    c.drawImage(str(png_path), 72, 600, width=80, height=80)
    c.save()
    png_path.unlink()
    _assert_size_under(out, 100_000)


def write_calrgb_pdf() -> None:
    """Hand-crafted PDF with a /CalRGB calibrated-color color space.

    reportlab does not expose CalRGB directly, so we emit the bytes manually.
    The page references a /CalRGB color space and fills a rectangle with it.
    PDFium must accept the calibrated profile and convert to RGB for output.
    """
    out = HERE / "calrgb-receipt.pdf"

    # Content stream: set CalRGB fill color (mid-gray), draw rectangle, draw text.
    content = (
        b"q\n"
        b"/CS0 cs\n"           # set fill color space to CalRGB (named in resources)
        b"0.6 0.6 0.6 sc\n"    # mid-gray fill
        b"72 700 200 60 re f\n"
        b"BT /F1 14 Tf 72 660 Td (CALRGB COLOR PDF) Tj ET\n"
        b"BT /F1 12 Tf 72 640 Td (TOTAL $1.00) Tj ET\n"
        b"Q\n"
    )

    objects: list[bytes] = []

    def add_obj(body: bytes) -> int:
        idx = len(objects) + 1
        objects.append(body)
        return idx

    # 1: Catalog
    catalog_idx = add_obj(b"<</Type /Catalog /Pages 2 0 R>>")
    # 2: Pages
    pages_idx = add_obj(b"<</Type /Pages /Kids [3 0 R] /Count 1>>")
    # 3: Page (forward refs to 4=Contents, 5=Font, 6=ColorSpace dict)
    page_body = (
        b"<</Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] "
        b"/Contents 4 0 R /Resources <</Font <</F1 5 0 R>> "
        b"/ColorSpace <</CS0 [/CalRGB <</WhitePoint [0.9505 1.0 1.089] "
        b"/Gamma [2.2 2.2 2.2] /Matrix [0.4124 0.2126 0.0193 "
        b"0.3576 0.7152 0.1192 0.1805 0.0722 0.9505]>>]>>>>>>"
    )
    page_idx = add_obj(page_body)
    # 4: Contents stream
    stream_body = b"<</Length " + str(len(content)).encode("ascii") + b">>\nstream\n" + content + b"endstream"
    contents_idx = add_obj(stream_body)
    # 5: Standard 14 font (Helvetica)
    font_idx = add_obj(b"<</Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding>>")

    # Assemble file
    parts: list[bytes] = []
    parts.append(b"%PDF-1.4\n")
    parts.append(b"%\xe2\xe3\xcf\xd3\n")

    offsets: list[int] = []
    cumulative = sum(len(p) for p in parts)
    for i, body in enumerate(objects, start=1):
        offsets.append(cumulative)
        obj_bytes = f"{i} 0 obj\n".encode("ascii") + body + b"\nendobj\n"
        parts.append(obj_bytes)
        cumulative += len(obj_bytes)

    xref_offset = cumulative
    xref = [b"xref\n", f"0 {len(objects) + 1}\n".encode("ascii"), b"0000000000 65535 f \n"]
    for o in offsets:
        xref.append(f"{o:010d} 00000 n \n".encode("ascii"))
    parts.extend(xref)
    trailer = (
        b"trailer\n"
        b"<</Size " + str(len(objects) + 1).encode("ascii") + b" /Root 1 0 R>>\n"
        b"startxref\n"
        + str(xref_offset).encode("ascii")
        + b"\n%%EOF\n"
    )
    parts.append(trailer)

    out.write_bytes(b"".join(parts))
    _assert_size_under(out, 100_000)


def write_devicen_pdf() -> None:
    """Hand-crafted PDF with a Lab (CIE-based) non-DeviceRGB color space.

    Complements CalRGB with a different non-RGB color space PDFium must handle.
    Uses /Lab — another CIE-based color space referenced by the page.
    """
    out = HERE / "lab-receipt.pdf"

    content = (
        b"q\n"
        b"/CS0 cs\n"
        b"50 0 0 sc\n"        # Lab L=50, a=0, b=0 (mid gray)
        b"72 700 200 60 re f\n"
        b"BT /F1 14 Tf 72 660 Td (LAB COLOR PDF) Tj ET\n"
        b"BT /F1 12 Tf 72 640 Td (TOTAL $1.00) Tj ET\n"
        b"Q\n"
    )

    objects: list[bytes] = []

    def add_obj(body: bytes) -> int:
        idx = len(objects) + 1
        objects.append(body)
        return idx

    add_obj(b"<</Type /Catalog /Pages 2 0 R>>")
    add_obj(b"<</Type /Pages /Kids [3 0 R] /Count 1>>")
    page_body = (
        b"<</Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] "
        b"/Contents 4 0 R /Resources <</Font <</F1 5 0 R>> "
        b"/ColorSpace <</CS0 [/Lab <</WhitePoint [0.9505 1.0 1.089] "
        b"/Range [-128 127 -128 127]>>]>>>>>>"
    )
    add_obj(page_body)
    stream_body = b"<</Length " + str(len(content)).encode("ascii") + b">>\nstream\n" + content + b"endstream"
    add_obj(stream_body)
    add_obj(b"<</Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding>>")

    parts: list[bytes] = []
    parts.append(b"%PDF-1.4\n")
    parts.append(b"%\xe2\xe3\xcf\xd3\n")

    offsets: list[int] = []
    cumulative = sum(len(p) for p in parts)
    for i, body in enumerate(objects, start=1):
        offsets.append(cumulative)
        obj_bytes = f"{i} 0 obj\n".encode("ascii") + body + b"\nendobj\n"
        parts.append(obj_bytes)
        cumulative += len(obj_bytes)

    xref_offset = cumulative
    xref = [b"xref\n", f"0 {len(objects) + 1}\n".encode("ascii"), b"0000000000 65535 f \n"]
    for o in offsets:
        xref.append(f"{o:010d} 00000 n \n".encode("ascii"))
    parts.extend(xref)
    trailer = (
        b"trailer\n"
        b"<</Size " + str(len(objects) + 1).encode("ascii") + b" /Root 1 0 R>>\n"
        b"startxref\n"
        + str(xref_offset).encode("ascii")
        + b"\n%%EOF\n"
    )
    parts.append(trailer)

    out.write_bytes(b"".join(parts))
    _assert_size_under(out, 100_000)


def _assert_size_under(path: Path, limit: int) -> None:
    size = path.stat().st_size
    if size >= limit:
        raise SystemExit(
            f"Fixture {path.name} exceeded size limit: {size} >= {limit}. "
            "Reduce content or compress further."
        )
    print(f"  {path.name}: {size} bytes")


def main() -> int:
    print("Generating PDF fixtures for RECEIPTS-651...")
    write_cmyk_pdf()
    write_indexed_pdf()
    write_calrgb_pdf()
    write_devicen_pdf()
    print("Done.")
    print()
    print("JBIG2 fixture intentionally NOT generated:")
    print("  JBIG2 encoding requires the external `jbig2enc` binary, which is")
    print("  not available on most developer machines. The corresponding test")
    print("  is marked with [Fact(Skip = ...)] in PdfConversionServiceFixtureTests.cs.")
    print("  See RECEIPTS-651 for tracking.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
