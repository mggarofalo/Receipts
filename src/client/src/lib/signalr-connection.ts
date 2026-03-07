let connectionId: string | null = null;

export function setConnectionId(id: string | null): void {
  connectionId = id;
}

export function getConnectionId(): string | null {
  return connectionId;
}
