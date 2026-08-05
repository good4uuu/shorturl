import { useEffect, useState, type FormEvent } from "react";
import QRCode from "qrcode";
import { createRoot } from "react-dom/client";
import "./styles.css";

type Link = {
  originalUrl: string;
  shortCode: string;
  shortUrl: string;
  createdAtUtc: string;
  lastAccessedAtUtc?: string;
  visitCount: number;
};

const apiBaseUrl = import.meta.env.VITE_API_URL?.replace(/\/$/, "") ?? "";

function App() {
  const [url, setUrl] = useState("");
  const [result, setResult] = useState<Link>();
  const [recent, setRecent] = useState<Link[]>([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [copied, setCopied] = useState(false);
  const [qr, setQr] = useState("");

  async function loadRecent() {
    const endpoint = `${apiBaseUrl}/api/urls?limit=8`;
    console.info("[ShortUrl] Loading recent links", { endpoint });

    try {
      const response = await fetch(endpoint);
      console.info("[ShortUrl] Recent links response", { status: response.status });

      if (response.ok) {
        const links: Link[] = await response.json();
        setRecent(links);
        setResult((currentResult) => {
          if (!currentResult) {
            return currentResult;
          }

          return links.find((link) => link.shortCode === currentResult.shortCode)
            ?? currentResult;
        });
      }
    } catch (error) {
      console.error("[ShortUrl] Could not load recent links", { endpoint, error });
    }
  }

  useEffect(() => {
    const refreshIntervalMs = 10_000;
    console.info("[ShortUrl] API base URL configured", {
      apiBaseUrl,
      refreshIntervalMs,
    });

    void loadRecent();
    const intervalId = window.setInterval(() => void loadRecent(), refreshIntervalMs);

    return () => window.clearInterval(intervalId);
  }, []);

  useEffect(() => {
    if (result) {
      void QRCode.toDataURL(result.shortUrl, { width: 180, margin: 1 }).then(setQr);
    } else {
      setQr("");
    }
  }, [result]);

  async function submit(e: FormEvent) {
    e.preventDefault();
    setError("");
    setResult(undefined);
    setLoading(true);

    const endpoint = `${apiBaseUrl}/api/urls`;
    console.info("[ShortUrl] Creating short link", { endpoint });

    try {
      const response = await fetch(endpoint, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ url }),
      });
      const body = await response.json();
      console.info("[ShortUrl] Create link response", { status: response.status });

      if (!response.ok) {
        throw new Error(
          body.error ?? "Unable to create a short link. Please try again.",
        );
      }

      setResult({
        ...body,
        visitCount: 0,
        createdAtUtc: new Date().toISOString(),
      });
      setUrl("");
      await loadRecent();
    } catch (error) {
      console.error("[ShortUrl] Create link request failed", { endpoint, error });
      setError(
        error instanceof Error
          ? error.message
          : "Unable to create a short link. Please try again.",
      );
    } finally {
      setLoading(false);
    }
  }

  async function copy(link: Link) {
    await navigator.clipboard.writeText(link.shortUrl);
    console.info("[ShortUrl] Short link copied", { shortCode: link.shortCode });
    setCopied(true);
    setTimeout(() => setCopied(false), 1500);
  }

  return (
    <main>
      <section className="card">
        <p className="eyebrow">SIMPLE · SECURE · FAST</p>
        <h1>Make every link count.</h1>
        <p className="intro">
          Paste a long URL and get a short, shareable link in seconds.
        </p>
        <form onSubmit={submit}>
          <label htmlFor="url">Your long URL</label>
          <div className="row">
            <input
              id="url"
              type="url"
              required
              value={url}
              onChange={(e) => setUrl(e.target.value)}
              placeholder="https://example.com/a/very/long/link"
            />
            <button disabled={loading}>
              {loading ? "Shortening…" : "Shorten URL"}
            </button>
          </div>
        </form>
        <div aria-live="polite">
          {error && <p className="error">{error}</p>}
          {result && (
            <section className="success">
              <p>Your shortened URL is ready</p>
              <a href={result.shortUrl} target="_blank" rel="noreferrer">
                {result.shortUrl}
              </a>
              <div className="actions">
                <button className="secondary" onClick={() => void copy(result)}>
                  {copied ? "Copied!" : "Copy link"}
                </button>
                <a className="open" href={result.shortUrl} target="_blank" rel="noreferrer">
                  Open link ?
                </a>
              </div>
              {qr && <img className="qr" src={qr} alt={`QR code for ${result.shortUrl}`} />}
            </section>
          )}
        </div>
        {recent.length > 0 && (
          <section className="recent">
            <h2>Recent links</h2>
            {recent.map((link) => (
              <article key={link.shortCode}>
                <a href={link.shortUrl} target="_blank" rel="noreferrer">
                  {link.shortUrl}
                </a>
                <span>
                  {link.visitCount} {link.visitCount === 1 ? "visit" : "visits"}
                </span>
                <button className="copy-small" onClick={() => void copy(link)}>
                  Copy
                </button>
              </article>
            ))}
          </section>
        )}
      </section>
    </main>
  );
}

createRoot(document.getElementById("root")!).render(<App />);


