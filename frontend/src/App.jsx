import { useEffect, useState } from "react";

const environments = ["Dev", "QA", "Staging", "Prod"];
const statuses = ["Pending", "Success", "Failed"];
const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5133").replace(/\/$/, "");

const emptyForm = {
  gameName: "",
  teamName: "",
  buildVersion: "",
  environment: "QA",
  status: "Pending",
  releaseNotes: ""
};

function App() {
  const [releases, setReleases] = useState([]);
  const [filters, setFilters] = useState({ environment: "All", status: "All" });
  const [formData, setFormData] = useState(emptyForm);
  const [health, setHealth] = useState({ status: "Checking", timestamp: "" });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isRefreshing, setIsRefreshing] = useState(true);
  const [errorMessage, setErrorMessage] = useState("");

  async function loadReleases(nextFilters = filters) {
    setIsRefreshing(true);
    setErrorMessage("");

    try {
      const query = new URLSearchParams();

      if (nextFilters.environment !== "All") {
        query.set("environment", nextFilters.environment);
      }

      if (nextFilters.status !== "All") {
        query.set("status", nextFilters.status);
      }

      const response = await fetch(
        `${apiBaseUrl}/api/releases${query.toString() ? `?${query.toString()}` : ""}`
      );

      if (!response.ok) {
        throw new Error("Unable to load releases.");
      }

      const data = await response.json();
      setReleases(data);
    } catch (error) {
      setErrorMessage(error.message || "Unable to load releases.");
    } finally {
      setIsRefreshing(false);
    }
  }

  async function loadHealth() {
    try {
      const response = await fetch(`${apiBaseUrl}/health`);

      if (!response.ok) {
        throw new Error();
      }

      const data = await response.json();
      setHealth({
        status: data.status,
        timestamp: data.timestamp
      });
    } catch {
      setHealth({
        status: "Unavailable",
        timestamp: ""
      });
    }
  }

  useEffect(() => {
    loadReleases(filters);
    loadHealth();
  }, []);

  const summary = {
    total: releases.length,
    success: releases.filter((release) => release.status === "Success").length,
    pending: releases.filter((release) => release.status === "Pending").length,
    failed: releases.filter((release) => release.status === "Failed").length
  };

  function handleFormChange(event) {
    const { name, value } = event.target;
    setFormData((current) => ({
      ...current,
      [name]: value
    }));
  }

  async function handleCreateRelease(event) {
    event.preventDefault();
    setIsSubmitting(true);
    setErrorMessage("");

    try {
      const response = await fetch(`${apiBaseUrl}/api/releases`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify(formData)
      });

      if (!response.ok) {
        throw new Error("Unable to create release.");
      }

      setFormData(emptyForm);
      await loadReleases(filters);
    } catch (error) {
      setErrorMessage(error.message || "Unable to create release.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleStatusUpdate(release, nextStatus) {
    setErrorMessage("");

    try {
      const response = await fetch(`${apiBaseUrl}/api/releases/${release.id}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify({
          gameName: release.gameName,
          teamName: release.teamName,
          buildVersion: release.buildVersion,
          environment: release.environment,
          status: nextStatus,
          releaseNotes: release.releaseNotes
        })
      });

      if (!response.ok) {
        throw new Error("Unable to update release status.");
      }

      await loadReleases(filters);
    } catch (error) {
      setErrorMessage(error.message || "Unable to update release status.");
    }
  }

  async function handleDeleteRelease(releaseId) {
    setErrorMessage("");

    try {
      const response = await fetch(`${apiBaseUrl}/api/releases/${releaseId}`, {
        method: "DELETE"
      });

      if (!response.ok) {
        throw new Error("Unable to delete release.");
      }

      await loadReleases(filters);
    } catch (error) {
      setErrorMessage(error.message || "Unable to delete release.");
    }
  }

  async function handleFilterChange(event) {
    const { name, value } = event.target;
    const nextFilters = {
      ...filters,
      [name]: value
    };

    setFilters(nextFilters);
    await loadReleases(nextFilters);
  }

  return (
    <div className="app-shell">
      <div className="hero-gradient" />
      <header className="page-header">
        <div>
          <p className="eyebrow">Internal Developer Tool</p>
          <h1>Game Release Dashboard</h1>
          <p className="lede">
            Track game and software releases across Dev, QA, Staging, and Prod with a
            simple deployment command center.
          </p>
        </div>
        <div className="health-card">
          <span className={`health-pill status-${health.status.toLowerCase()}`}>
            API {health.status}
          </span>
          <p>
            {health.timestamp
              ? `Last health check: ${formatDate(health.timestamp)}`
              : "Waiting for backend health telemetry."}
          </p>
        </div>
      </header>

      <section className="summary-grid">
        <SummaryCard label="Total Releases" value={summary.total} tone="neutral" />
        <SummaryCard label="Successful" value={summary.success} tone="success" />
        <SummaryCard label="Pending" value={summary.pending} tone="pending" />
        <SummaryCard label="Failed" value={summary.failed} tone="failed" />
      </section>

      {errorMessage ? <div className="banner error">{errorMessage}</div> : null}

      <main className="dashboard-layout">
        <section className="panel">
          <div className="panel-heading">
            <div>
              <p className="section-label">Create Release</p>
              <h2>Add a new deployment record</h2>
            </div>
          </div>

          <form className="release-form" onSubmit={handleCreateRelease}>
            <label>
              Game Name
              <input
                name="gameName"
                value={formData.gameName}
                onChange={handleFormChange}
                placeholder="Battlefield"
                required
              />
            </label>

            <label>
              Team Name
              <input
                name="teamName"
                value={formData.teamName}
                onChange={handleFormChange}
                placeholder="Core Services"
                required
              />
            </label>

            <label>
              Build Version
              <input
                name="buildVersion"
                value={formData.buildVersion}
                onChange={handleFormChange}
                placeholder="1.0.4"
                required
              />
            </label>

            <div className="field-row">
              <label>
                Environment
                <select
                  name="environment"
                  value={formData.environment}
                  onChange={handleFormChange}
                >
                  {environments.map((environment) => (
                    <option key={environment} value={environment}>
                      {environment}
                    </option>
                  ))}
                </select>
              </label>

              <label>
                Status
                <select name="status" value={formData.status} onChange={handleFormChange}>
                  {statuses.map((status) => (
                    <option key={status} value={status}>
                      {status}
                    </option>
                  ))}
                </select>
              </label>
            </div>

            <label>
              Release Notes
              <textarea
                name="releaseNotes"
                value={formData.releaseNotes}
                onChange={handleFormChange}
                rows="5"
                placeholder="Testing matchmaking service changes"
                required
              />
            </label>

            <button className="primary-button" type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Saving..." : "Create Release"}
            </button>
          </form>
        </section>

        <section className="panel panel-wide">
          <div className="panel-heading panel-heading-split">
            <div>
              <p className="section-label">Release Inventory</p>
              <h2>All tracked releases</h2>
            </div>

            <div className="filters">
              <label>
                Environment
                <select
                  name="environment"
                  value={filters.environment}
                  onChange={handleFilterChange}
                >
                  <option value="All">All</option>
                  {environments.map((environment) => (
                    <option key={environment} value={environment}>
                      {environment}
                    </option>
                  ))}
                </select>
              </label>

              <label>
                Status
                <select name="status" value={filters.status} onChange={handleFilterChange}>
                  <option value="All">All</option>
                  {statuses.map((status) => (
                    <option key={status} value={status}>
                      {status}
                    </option>
                  ))}
                </select>
              </label>
            </div>
          </div>

          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Game Name</th>
                  <th>Team</th>
                  <th>Build Version</th>
                  <th>Environment</th>
                  <th>Status</th>
                  <th>Created Date</th>
                  <th>Release Notes</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {isRefreshing ? (
                  <tr>
                    <td colSpan="8" className="empty-state">
                      Loading releases...
                    </td>
                  </tr>
                ) : releases.length === 0 ? (
                  <tr>
                    <td colSpan="8" className="empty-state">
                      No releases match the selected filters.
                    </td>
                  </tr>
                ) : (
                  releases.map((release) => (
                    <tr key={release.id}>
                      <td>{release.gameName}</td>
                      <td>{release.teamName}</td>
                      <td className="mono">{release.buildVersion}</td>
                      <td>{release.environment}</td>
                      <td>
                        <span className={`status-badge ${release.status.toLowerCase()}`}>
                          {release.status}
                        </span>
                      </td>
                      <td>{formatDate(release.createdAt)}</td>
                      <td className="notes-cell">{release.releaseNotes}</td>
                      <td>
                        <div className="action-group">
                          <button
                            className="ghost-button"
                            type="button"
                            disabled={release.status === "Success"}
                            onClick={() => handleStatusUpdate(release, "Success")}
                          >
                            Mark Success
                          </button>
                          <button
                            className="ghost-button ghost-button-failed"
                            type="button"
                            disabled={release.status === "Failed"}
                            onClick={() => handleStatusUpdate(release, "Failed")}
                          >
                            Mark Failed
                          </button>
                          <button
                            className="link-button"
                            type="button"
                            onClick={() => handleDeleteRelease(release.id)}
                          >
                            Delete
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </section>
      </main>
    </div>
  );
}

function SummaryCard({ label, value, tone }) {
  return (
    <article className={`summary-card tone-${tone}`}>
      <p>{label}</p>
      <strong>{value}</strong>
    </article>
  );
}

function formatDate(value) {
  return new Intl.DateTimeFormat("en-US", {
    dateStyle: "medium",
    timeStyle: "short"
  }).format(new Date(value));
}

export default App;
