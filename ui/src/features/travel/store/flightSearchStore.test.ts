import { describe, it, expect, beforeEach } from 'vitest';
import { useFlightSearchStore } from './flightSearchStore';
import type { FlightSearchResponse } from '../services/flightSearchService';

const mockSearch: FlightSearchResponse = {
    id: 'search-1',
    createdAt: '2026-06-01T10:00:00Z',
    flightOptions: [],
};

beforeEach(() => {
    useFlightSearchStore.setState({ flightSearch: null });
});

describe('flightSearchStore', () => {
    it('starts with no flight search', () => {
        expect(useFlightSearchStore.getState().flightSearch).toBeNull();
    });

    it('setFlightSearch stores the result', () => {
        useFlightSearchStore.getState().setFlightSearch(mockSearch);

        expect(useFlightSearchStore.getState().flightSearch).toEqual(mockSearch);
    });

    it('clearFlightSearch resets to null', () => {
        useFlightSearchStore.getState().setFlightSearch(mockSearch);
        useFlightSearchStore.getState().clearFlightSearch();

        expect(useFlightSearchStore.getState().flightSearch).toBeNull();
    });
});
