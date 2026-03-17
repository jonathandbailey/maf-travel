import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import DashboardPage from './DashboardPage';
import * as travelPlanService from '../features/travel/services/travelPlanService';
import type { TravelPlanResponse } from '../features/travel/services/travelPlanService';

const mockNavigate = vi.fn();

vi.mock('react-router-dom', () => ({
    useNavigate: () => mockNavigate,
}));

vi.mock('../features/travel/services/travelPlanService', () => ({
    listTravelPlans: vi.fn(),
    createTravelPlan: vi.fn(),
}));

const mockPlans: TravelPlanResponse[] = [
    {
        id: 'plan-1',
        destination: 'Paris',
        origin: 'New York',
        startDate: '2026-06-01',
        endDate: '2026-06-14',
        numberOfTravelers: 2,
        sessionId: null,
    },
    {
        id: 'plan-2',
        destination: null,
        origin: null,
        startDate: null,
        endDate: null,
        numberOfTravelers: null,
        sessionId: null,
    },
];

beforeEach(() => {
    vi.clearAllMocks();
    mockNavigate.mockClear();
});

describe('DashboardPage', () => {
    it('shows skeleton cards while loading', () => {
        vi.mocked(travelPlanService.listTravelPlans).mockReturnValue(new Promise(() => {}));

        render(<DashboardPage />);

        expect(document.querySelector('.ant-skeleton')).toBeTruthy();
    });

    it('shows empty state when no plans exist', async () => {
        vi.mocked(travelPlanService.listTravelPlans).mockResolvedValue([]);

        render(<DashboardPage />);

        await waitFor(() =>
            expect(screen.getByText(/No travel plans yet/i)).toBeInTheDocument()
        );
    });

    it('renders plan cards when plans exist', async () => {
        vi.mocked(travelPlanService.listTravelPlans).mockResolvedValue(mockPlans);

        render(<DashboardPage />);

        await waitFor(() =>
            expect(screen.getByText('Paris')).toBeInTheDocument()
        );
        expect(screen.getByText('No destination set')).toBeInTheDocument();
    });

    it('shows error alert when API call fails', async () => {
        vi.mocked(travelPlanService.listTravelPlans).mockRejectedValue(new Error('Network error'));

        render(<DashboardPage />);

        await waitFor(() =>
            expect(screen.getByText('Network error')).toBeInTheDocument()
        );
    });

    it('navigates to plan page when card is clicked', async () => {
        vi.mocked(travelPlanService.listTravelPlans).mockResolvedValue(mockPlans);

        render(<DashboardPage />);

        await waitFor(() => screen.getByText('Paris'));
        await userEvent.click(screen.getByText('Paris'));

        expect(mockNavigate).toHaveBeenCalledWith('/travel-plans/plan-1');
    });

    it('navigates to new plan after creating one', async () => {
        vi.mocked(travelPlanService.listTravelPlans).mockResolvedValue([]);
        vi.mocked(travelPlanService.createTravelPlan).mockResolvedValue({
            id: 'new-plan',
            destination: null,
            origin: null,
            startDate: null,
            endDate: null,
            numberOfTravelers: null,
            sessionId: null,
        });

        render(<DashboardPage />);

        await waitFor(() => screen.getByText(/No travel plans yet/i));
        await userEvent.click(screen.getByRole('button', { name: /New Travel Plan/i }));

        await waitFor(() =>
            expect(mockNavigate).toHaveBeenCalledWith('/travel-plans/new-plan')
        );
    });
});
