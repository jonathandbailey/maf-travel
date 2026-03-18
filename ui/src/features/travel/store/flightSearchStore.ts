import { create } from 'zustand';
import type { FlightSearchResponse } from '../services/flightSearchService';

interface FlightSearchStore {
    flightSearches: FlightSearchResponse[];
    addFlightSearch: (search: FlightSearchResponse) => void;
    clearFlightSearches: () => void;
}

export const useFlightSearchStore = create<FlightSearchStore>((set) => ({
    flightSearches: [],
    addFlightSearch: (search) => set((state) => ({ flightSearches: [...state.flightSearches, search] })),
    clearFlightSearches: () => set({ flightSearches: [] }),
}));
