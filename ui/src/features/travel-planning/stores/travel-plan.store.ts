import { create } from 'zustand';
import type { TravelPlan } from '../domain/TravelPlan';

interface TravelPlanStore {
    travelPlans: TravelPlan[];
    addTravelPlan: (travelPlan: TravelPlan) => void;
    getTravelPlanById: (id: string) => TravelPlan | undefined;
    updateTravelPlan: (travelPlan: TravelPlan) => void;
    removeTravelPlan: (id: string) => void;
    clear: () => void;
}

export const useTravelPlanStore = create<TravelPlanStore>((set, get) => ({
    travelPlans: [],

    addTravelPlan: (travelPlan: TravelPlan) =>
        set((state) => {
            // Check if travel plan with this ID already exists
            const existsIndex = state.travelPlans.findIndex(tp => tp.id === travelPlan.id);

            if (existsIndex !== -1) {
                // Replace existing travel plan
                const updatedTravelPlans = [...state.travelPlans];
                updatedTravelPlans[existsIndex] = travelPlan;
                return { travelPlans: updatedTravelPlans };
            }

            // Add new travel plan if it doesn't exist
            return { travelPlans: [...state.travelPlans, travelPlan] };
        }),

    getTravelPlanById: (id: string) => {
        return get().travelPlans.find(tp => tp.id === id);
    },

    updateTravelPlan: (travelPlan: TravelPlan) =>
        set((state) => {
            const updatedTravelPlans = state.travelPlans.map(tp =>
                tp.id === travelPlan.id ? travelPlan : tp
            );
            return { travelPlans: updatedTravelPlans };
        }),

    removeTravelPlan: (id: string) =>
        set((state) => ({
            travelPlans: state.travelPlans.filter(tp => tp.id !== id)
        })),

    clear: () =>
        set({ travelPlans: [] })
}));