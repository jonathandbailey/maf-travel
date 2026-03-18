import { create } from 'zustand';
import type { ExchangeItem } from '../hooks/useChatAgent';

interface ChatStore {
    exchanges: ExchangeItem[];
    addExchange: (exchange: ExchangeItem) => void;
    clearExchanges: () => void;
}

export const useChatStore = create<ChatStore>((set) => ({
    exchanges: [],
    addExchange: (exchange) =>
        set((state) => ({ exchanges: [...state.exchanges, exchange] })),
    clearExchanges: () => set({ exchanges: [] }),
}));
