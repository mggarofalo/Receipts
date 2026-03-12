#!/usr/bin/env dotnet

string repoRoot = GetRepoRoot();
string modelDir = Path.Combine(repoRoot, "src", "Infrastructure", "Models", "AllMiniLmL6V2");
Directory.CreateDirectory(modelDir);

string onnxUrl = "https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/resolve/main/onnx/model.onnx";
string vocabUrl = "https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/resolve/main/vocab.txt";

using HttpClient http = new();

string onnxPath = Path.Combine(modelDir, "model.onnx");
if (!File.Exists(onnxPath))
{
    Console.WriteLine("Downloading all-MiniLM-L6-v2 ONNX model...");
    using Stream onnxStream = await http.GetStreamAsync(onnxUrl);
    using FileStream onnxFs = File.Create(onnxPath);
    await onnxStream.CopyToAsync(onnxFs);
    Console.WriteLine("Done.");
}
else
{
    Console.WriteLine("model.onnx already exists, skipping.");
}

string vocabPath = Path.Combine(modelDir, "vocab.txt");
if (!File.Exists(vocabPath))
{
    Console.WriteLine("Downloading vocab.txt...");
    using Stream vocabStream = await http.GetStreamAsync(vocabUrl);
    using FileStream vocabFs = File.Create(vocabPath);
    await vocabStream.CopyToAsync(vocabFs);
    Console.WriteLine("Done.");
}
else
{
    Console.WriteLine("vocab.txt already exists, skipping.");
}

Console.WriteLine($"ONNX model files ready at {modelDir}");
return 0;

static string GetRepoRoot()
{
    System.Diagnostics.ProcessStartInfo psi = new("git", ["rev-parse", "--show-toplevel"])
    {
        RedirectStandardOutput = true,
        UseShellExecute = false,
    };

    using System.Diagnostics.Process? proc = System.Diagnostics.Process.Start(psi);
    string output = proc!.StandardOutput.ReadToEnd().Trim();
    proc.WaitForExit();
    return output;
}
