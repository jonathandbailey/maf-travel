export interface Session {
    id: string;
    createdAt: string;
}

export async function createSession(): Promise<Session> {
    const response = await fetch(`https://localhost:7168/sessions`, { method: 'POST' });
    if (!response.ok) throw new Error(`Failed to create session: ${response.status}`);
    return response.json();
}

export async function updateSession(sessionId: string, travelPlanId: string): Promise<Session> {
    const response = await fetch(`https://localhost:7168/sessions/${sessionId}`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ travelPlanId }),
    });
    if (!response.ok) throw new Error(`Failed to update session: ${response.status}`);
    return response.json();
}
