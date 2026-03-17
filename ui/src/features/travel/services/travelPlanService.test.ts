import { describe, it, expect, vi, beforeEach } from 'vitest';
import { createTravelPlan, listTravelPlans, getTravelPlan } from './travelPlanService';

const mockPlan = {
    id: 'plan-1',
    origin: 'New York',
    destination: 'Paris',
    numberOfTravelers: 2,
    startDate: '2026-06-01',
    endDate: '2026-06-14',
    sessionId: null,
};

function mockFetch(data: unknown, ok = true, status = 200) {
    return vi.fn().mockResolvedValue({
        ok,
        status,
        json: () => Promise.resolve(data),
    });
}

beforeEach(() => {
    vi.restoreAllMocks();
});

describe('createTravelPlan', () => {
    it('POSTs to /api/travel/plans with JSON content type', async () => {
        globalThis.fetch = mockFetch(mockPlan);

        await createTravelPlan();

        expect(fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/travel/plans'),
            expect.objectContaining({ method: 'POST', headers: { 'Content-Type': 'application/json' } })
        );
    });

    it('returns the created plan', async () => {
        globalThis.fetch = mockFetch(mockPlan);

        const result = await createTravelPlan();

        expect(result).toEqual(mockPlan);
    });

    it('throws when response is not ok', async () => {
        globalThis.fetch = mockFetch({}, false, 400);

        await expect(createTravelPlan()).rejects.toThrow('400');
    });
});

describe('listTravelPlans', () => {
    it('GETs /api/travel/plans', async () => {
        globalThis.fetch = mockFetch([mockPlan]);

        await listTravelPlans();

        expect(fetch).toHaveBeenCalledWith(expect.stringContaining('/api/travel/plans'));
    });

    it('returns the list of plans', async () => {
        globalThis.fetch = mockFetch([mockPlan]);

        const result = await listTravelPlans();

        expect(result).toEqual([mockPlan]);
    });

    it('throws when response is not ok', async () => {
        globalThis.fetch = mockFetch({}, false, 500);

        await expect(listTravelPlans()).rejects.toThrow('500');
    });
});

describe('getTravelPlan', () => {
    it('GETs the correct plan URL', async () => {
        globalThis.fetch = mockFetch(mockPlan);

        await getTravelPlan('plan-1');

        expect(fetch).toHaveBeenCalledWith(expect.stringContaining('/api/travel/plans/plan-1'));
    });

    it('returns the plan', async () => {
        globalThis.fetch = mockFetch(mockPlan);

        const result = await getTravelPlan('plan-1');

        expect(result).toEqual(mockPlan);
    });

    it('throws when response is not ok', async () => {
        globalThis.fetch = mockFetch({}, false, 404);

        await expect(getTravelPlan('plan-1')).rejects.toThrow('404');
    });
});
