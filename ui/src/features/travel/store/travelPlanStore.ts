import { create } from 'zustand';
import type { TravelPlan } from '../domain/TravelPlan';

interface TravelPlanStore {
    plan: TravelPlan | null;
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
    createPlan: () => set({ plan: emptyPlan() }),
    updatePlan: (updates) =>
        set((state) => ({
            plan: state.plan ? { ...state.plan, ...updates } : { ...emptyPlan(), ...updates },
        })),
}));
