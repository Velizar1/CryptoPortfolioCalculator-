import { useState, FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { Config } from "../helpers/ConfigHelper";
import { uploadFile } from "../services/PortfolioService";
import "./UploadPage.css";

export default function UploadPage() {
  const [file, setFile] = useState<File>();
  const [busy, setBusy] = useState(false);
  const nav = useNavigate();

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!file) return;

    try {
      const fd = new FormData();
      fd.append("file", file);
      setBusy(true);

      const res = await uploadFile(fd);
      setBusy(false);

      if (!res.ok) {
        alert(await res.text());
        return;
      }

      const payload = await res.json();
      nav("/portfolio", { state: payload });
    } catch (err) {
      alert(err);
      setBusy(false);
    }
  }

  return (
    <main className="page">
      <h1>Upload your portfolio file</h1>

      <form onSubmit={handleSubmit} className="mt-4 flex flex-col gap-4">
        <input
          type="file"
          accept=".txt"
          onChange={(e) => setFile(e.target.files?.[0])}
        />

        <button type="submit" disabled={!file || busy}>
          {busy ? "Uploading…" : "Upload & View"}
        </button>
      </form>
    </main>
  );
}
