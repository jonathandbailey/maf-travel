export interface TravelPlanResponse {
    id: string;
    origin: string | null;
    destination: string | null;
    numberOfTravelers: number | null;
    startDate: string | null;
    endDate: string | null;
}

const BASE = `https://localhost:7168`;

export async function createTravelPlan(): Promise<TravelPlanResponse> {
    const response = await fetch(`${BASE}/travel-plans`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({}),
    });
    if (!response.ok) throw new Error(`Failed to create travel plan: ${response.status}`);
    return response.json();
}

export async function listTravelPlans(): Promise<TravelPlanResponse[]> {
    const response = await fetch(`${BASE}/travel-plans`);
    if (!response.ok) throw new Error(`Failed to list travel plans: ${response.status}`);
    return response.json();
}
