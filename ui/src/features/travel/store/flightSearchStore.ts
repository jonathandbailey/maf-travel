import { create } from 'zustand';
import type { FlightSearchResponse } from '../services/flightSearchService';

interface FlightSearchStore {
    flightSearch: FlightSearchResponse | null;
    setFlightSearch: (search: FlightSearchResponse) => void;
    clearFlightSearch: () => void;
}

export const useFlightSearchStore = create<FlightSearchStore>((set) => ({
    flightSearch: null,
    setFlightSearch: (search) => set({ flightSearch: search }),
    clearFlightSearch: () => set({ flightSearch: null }),
}));
