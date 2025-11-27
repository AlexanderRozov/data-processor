import { useCallback, useEffect, useMemo, useState } from "react";

type EventRecord = {
  id: string;
  createdAt: string;
  value: number;
};

const API_BASE =
  import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, "") || "http://localhost:8080";
const DEFAULT_INTERVAL = 5000;

function formatDate(iso: string) {
  const date = new Date(iso);
  return `${date.toLocaleDateString()} ${date.toLocaleTimeString()}`;
}

function App() {
  const [events, setEvents] = useState<EventRecord[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [refreshMs, setRefreshMs] = useState(DEFAULT_INTERVAL);

  const fetchEvents = useCallback(async () => {
    setIsLoading(true);
    try {
      const response = await fetch(`${API_BASE}/events`);
      if (!response.ok) {
        throw new Error(`API responded with ${response.status}`);
      }
      const payload = (await response.json()) as EventRecord[];
      setEvents(payload);
      setError(null);
    } catch (err) {
      const message = err instanceof Error ? err.message : "Unknown error";
      setError(message);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchEvents();
  }, [fetchEvents]);

  useEffect(() => {
    const timer = setInterval(fetchEvents, refreshMs);
    return () => clearInterval(timer);
  }, [fetchEvents, refreshMs]);

  const summary = useMemo(() => {
    if (events.length === 0) {
      return { avg: 0, min: 0, max: 0 };
    }
    const values = events.map((e) => e.value);
    const sum = values.reduce((acc, value) => acc + value, 0);
    return {
      avg: Math.round((sum / values.length) * 100) / 100,
      min: Math.min(...values),
      max: Math.max(...values)
    };
  }, [events]);

  return (
    <main className="card">
      <h1>Data Processor Events</h1>
      <p className="subtitle">
        Streaming table backed by RabbitMQ → PostgreSQL. API host:{" "}
        <span className="tag">{API_BASE}</span>
      </p>

      <section className="controls">
        <div className="refresh-rate">
          <label htmlFor="refresh">Refresh (ms)</label>
          <input
            id="refresh"
            type="number"
            min={1000}
            max={60000}
            step={500}
            value={refreshMs}
            onChange={(event) => setRefreshMs(Number(event.target.value))}
          />
        </div>
        <button onClick={fetchEvents} disabled={isLoading}>
          {isLoading ? "Refreshing..." : "Refresh now"}
        </button>
        <span className="status">
          {error ? `⚠ ${error}` : `Loaded ${events.length} events | avg ${summary.avg}`}
        </span>
      </section>

      <section className="table-container">
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Created at</th>
              <th>Value</th>
            </tr>
          </thead>
          <tbody>
            {events.length === 0 && (
              <tr>
                <td colSpan={3}>{isLoading ? "Loading…" : "No events yet."}</td>
              </tr>
            )}
            {events.map((event) => (
              <tr key={event.id}>
                <td>{event.id}</td>
                <td>{formatDate(event.createdAt)}</td>
                <td>{event.value}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </main>
  );
}

export default App;

