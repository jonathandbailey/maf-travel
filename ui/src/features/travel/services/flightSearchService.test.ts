import { describe, it, expect, vi, beforeEach } from 'vitest';
import { getFlightSearch } from './flightSearchService';

const mockFlightSearch = {
    id: 'search-1',
    createdAt: '2026-06-01T10:00:00Z',
    flightOptions: [
        {
            id: 'flight-1',
            flightNumber: 'AA100',
            airline: 'American Airlines',
            origin: 'JFK',
            destination: 'CDG',
            departureTime: '2026-06-01T08:00:00Z',
            arrivalTime: '2026-06-01T20:00:00Z',
            pricePerPerson: 850,
            availableSeats: 12,
        },
    ],
};

beforeEach(() => {
    vi.restoreAllMocks();
});

describe('getFlightSearch', () => {
    it('GETs the correct flight search URL', async () => {
        globalThis.fetch = vi.fn().mockResolvedValue({
            ok: true,
            json: () => Promise.resolve(mockFlightSearch),
        });

        await getFlightSearch('search-1');

        expect(fetch).toHaveBeenCalledWith(expect.stringContaining('/api/flight-searches/search-1'));
    });

    it('returns the flight search result', async () => {
        globalThis.fetch = vi.fn().mockResolvedValue({
            ok: true,
            json: () => Promise.resolve(mockFlightSearch),
        });

        const result = await getFlightSearch('search-1');

        expect(result).toEqual(mockFlightSearch);
    });

    it('throws when response is not ok', async () => {
        globalThis.fetch = vi.fn().mockResolvedValue({
            ok: false,
            status: 404,
            json: () => Promise.resolve({}),
        });

        await expect(getFlightSearch('search-1')).rejects.toThrow('404');
    });
});
