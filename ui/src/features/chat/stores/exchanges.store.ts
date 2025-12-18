import { create } from 'zustand';
import type { Exchange } from '../domain/Exchange';

interface ExchangesStore {
    exchanges: Exchange[];
    addExchange: (exchange: Exchange) => void;
    clear: () => void;
}

export const useExchangesStore = create<ExchangesStore>((set) => ({
    exchanges: [],

    addExchange: (exchange: Exchange) =>
        set((state) => ({
            exchanges: [...state.exchanges, exchange]
        })),

    clear: () =>
        set({ exchanges: [] })
}));
