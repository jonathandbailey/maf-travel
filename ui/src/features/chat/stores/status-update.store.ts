import { create } from 'zustand';
import type { StatusUpdate } from '../domain/StatusUpdate';

interface StatusUpdateStore {
    statusUpdates: StatusUpdate[];
    currentStatusUpdate: StatusUpdate | null;
    addStatusUpdate: (statusUpdate: StatusUpdate) => void;
    setCurrentStatusUpdate: (statusUpdate: StatusUpdate | null) => void;
    clearStatusUpdates: () => void;
    getLatestBySource: (source: string) => StatusUpdate | null;
}

export const useStatusUpdateStore = create<StatusUpdateStore>((set, get) => ({
    statusUpdates: [],
    currentStatusUpdate: null,

    addStatusUpdate: (statusUpdate: StatusUpdate) =>
        set((state) => {
            const updatedStatusUpdates = [...state.statusUpdates, statusUpdate];
            return {
                statusUpdates: updatedStatusUpdates,
                currentStatusUpdate: statusUpdate,
            };
        }),

    setCurrentStatusUpdate: (statusUpdate: StatusUpdate | null) =>
        set({ currentStatusUpdate: statusUpdate }),

    clearStatusUpdates: () =>
        set({
            statusUpdates: [],
            currentStatusUpdate: null
        }),

    getLatestBySource: (source: string) => {
        const { statusUpdates } = get();
        const filteredUpdates = statusUpdates.filter(update => update.source === source);
        return filteredUpdates.length > 0 ? filteredUpdates[filteredUpdates.length - 1] : null;
    },
}));