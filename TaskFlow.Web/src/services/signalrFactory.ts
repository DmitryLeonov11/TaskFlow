import * as signalR from '@microsoft/signalr';

const INITIAL_RECONNECT_DELAY_MS = 2_000;
const MAX_RECONNECT_DELAY_MS = 60_000;

export function createSignalRService(
  hubPath: string,
  registerHandlers: (connection: signalR.HubConnection) => void
) {
  let connection: signalR.HubConnection | null = null;
  let reconnectTimer: ReturnType<typeof setTimeout> | null = null;
  let attempt = 0;
  // Bumped on every stop/start to invalidate in-flight starts and pending reconnects.
  let epoch = 0;

  async function startConnection() {
    if (connection) return;
    const myEpoch = ++epoch;

    const next = new signalR.HubConnectionBuilder()
      .withUrl(
        (import.meta.env.VITE_SIGNALR_HUB_URL || 'http://localhost:5000/hubs') + hubPath,
        { accessTokenFactory: () => localStorage.getItem('auth_token') || '' }
      )
      .withAutomaticReconnect()
      .build();

    registerHandlers(next);
    connection = next;

    next.onclose(async () => {
      if (connection !== next) return;
      connection = null;
      epoch += 1;
      attempt = 0;
      if (reconnectTimer) {
        clearTimeout(reconnectTimer);
        reconnectTimer = null;
      }
      const delay = Math.min(INITIAL_RECONNECT_DELAY_MS * 2 ** attempt, MAX_RECONNECT_DELAY_MS);
      attempt += 1;
      reconnectTimer = setTimeout(() => {
        reconnectTimer = null;
        if (myEpoch !== epoch) return;
        void startConnection();
      }, delay);
    });

    try {
      await next.start();
      if (myEpoch !== epoch) {
        // Stopped while we were starting — drop this connection.
        await next.stop().catch(() => {});
        return;
      }
      attempt = 0;
    } catch (err) {
      console.error(`SignalR ${hubPath} error:`, err);
      if (myEpoch !== epoch) return;

      connection = null;
      const delay = Math.min(INITIAL_RECONNECT_DELAY_MS * 2 ** attempt, MAX_RECONNECT_DELAY_MS);
      attempt += 1;
      reconnectTimer = setTimeout(() => {
        reconnectTimer = null;
        if (myEpoch !== epoch) return;
        void startConnection();
      }, delay);
    }
  }

  async function stopConnection() {
    epoch += 1;
    attempt = 0;
    if (reconnectTimer) {
      clearTimeout(reconnectTimer);
      reconnectTimer = null;
    }
    if (connection) {
      const c = connection;
      connection = null;
      await c.stop().catch(() => {});
    }
  }

  return { startConnection, stopConnection };
}
