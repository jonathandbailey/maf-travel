export interface Session {
    id: string;
    createdAt: string;
}

export async function createSession(): Promise<Session> {
    const response = await fetch(`https://localhost:7168/sessions`, { method: 'POST' });
    if (!response.ok) throw new Error(`Failed to create session: ${response.status}`);
    return response.json();
}
