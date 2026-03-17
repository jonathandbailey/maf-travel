import { describe, it, expect, beforeEach } from 'vitest';
import { useTravelPlanStore } from './travelPlanStore';

beforeEach(() => {
    useTravelPlanStore.setState({ plan: null, planVersion: 0 });
});

describe('travelPlanStore', () => {
    describe('createPlan', () => {
        it('increments planVersion', () => {
            useTravelPlanStore.getState().createPlan();

            expect(useTravelPlanStore.getState().planVersion).toBe(1);
        });

        it('sets plan to empty plan', () => {
            useTravelPlanStore.getState().createPlan();

            expect(useTravelPlanStore.getState().plan).toEqual({
                origin: null,
                destination: null,
                startDate: null,
                endDate: null,
                numberOfTravelers: null,
            });
        });

        it('increments planVersion on each call', () => {
            useTravelPlanStore.getState().createPlan();
            useTravelPlanStore.getState().createPlan();

            expect(useTravelPlanStore.getState().planVersion).toBe(2);
        });
    });

    describe('updatePlan', () => {
        it('merges updates into existing plan', () => {
            useTravelPlanStore.getState().createPlan();
            useTravelPlanStore.getState().updatePlan({ destination: 'Paris' });

            const { plan } = useTravelPlanStore.getState();
            expect(plan?.destination).toBe('Paris');
            expect(plan?.origin).toBeNull();
        });

        it('creates plan from empty state if plan is null', () => {
            useTravelPlanStore.getState().updatePlan({ origin: 'New York' });

            const { plan } = useTravelPlanStore.getState();
            expect(plan?.origin).toBe('New York');
            expect(plan?.destination).toBeNull();
        });

        it('does not change planVersion', () => {
            useTravelPlanStore.getState().createPlan();
            const versionBefore = useTravelPlanStore.getState().planVersion;

            useTravelPlanStore.getState().updatePlan({ destination: 'London' });

            expect(useTravelPlanStore.getState().planVersion).toBe(versionBefore);
        });

        it('does not overwrite unrelated fields', () => {
            useTravelPlanStore.getState().updatePlan({ origin: 'NYC', destination: 'Paris' });
            useTravelPlanStore.getState().updatePlan({ numberOfTravelers: 3 });

            const { plan } = useTravelPlanStore.getState();
            expect(plan?.origin).toBe('NYC');
            expect(plan?.destination).toBe('Paris');
            expect(plan?.numberOfTravelers).toBe(3);
        });
    });
});
