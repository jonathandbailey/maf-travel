export interface FlightOptionResponse {
    id: string;
    flightNumber: string;
    airline: string;
    origin: string;
    destination: string;
    departureTime: string;
    arrivalTime: string;
    pricePerPerson: number;
    availableSeats: number;
}

export interface FlightSearchResponse {
    id: string;
    createdAt: string;
    flightOptions: FlightOptionResponse[];
}

const BASE = `${import.meta.env.VITE_API_BASE_URL}/api`;

export async function getFlightSearch(id: string): Promise<FlightSearchResponse> {
    const response = await fetch(`${BASE}/flight-searches/${id}`);
    if (!response.ok) throw new Error(`Failed to get flight search: ${response.status}`);
    return response.json();
}
