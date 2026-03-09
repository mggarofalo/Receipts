#!/usr/bin/env bash
set -euo pipefail

MODEL_DIR="$(cd "$(dirname "$0")/../src/Infrastructure/Models/AllMiniLmL6V2" && pwd)"
mkdir -p "$MODEL_DIR"

ONNX_URL="https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/resolve/main/onnx/model.onnx"
VOCAB_URL="https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/resolve/main/vocab.txt"

if [ ! -f "$MODEL_DIR/model.onnx" ]; then
  echo "Downloading all-MiniLM-L6-v2 ONNX model..."
  curl -L -o "$MODEL_DIR/model.onnx" "$ONNX_URL"
  echo "Done."
else
  echo "model.onnx already exists, skipping."
fi

if [ ! -f "$MODEL_DIR/vocab.txt" ]; then
  echo "Downloading vocab.txt..."
  curl -L -o "$MODEL_DIR/vocab.txt" "$VOCAB_URL"
  echo "Done."
else
  echo "vocab.txt already exists, skipping."
fi

echo "ONNX model files ready at $MODEL_DIR"
