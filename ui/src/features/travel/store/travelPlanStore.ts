import { create } from 'zustand';
import type { TravelPlan } from '../domain/TravelPlan';

interface TravelPlanStore {
    plan: TravelPlan | null;
    planVersion: number;
    createPlan: () => void;
    updatePlan: (updates: Partial<TravelPlan>) => void;
}

const emptyPlan = (): TravelPlan => ({
    origin: null,
    destination: null,
    startDate: null,
    endDate: null,
    numberOfTravelers: null,
});

export const useTravelPlanStore = create<TravelPlanStore>((set) => ({
    plan: null,
    planVersion: 0,
    createPlan: () => set((state) => ({ plan: emptyPlan(), planVersion: state.planVersion + 1 })),
    updatePlan: (updates) =>
        set((state) => ({
            plan: state.plan ? { ...state.plan, ...updates } : { ...emptyPlan(), ...updates },
        })),
}));
