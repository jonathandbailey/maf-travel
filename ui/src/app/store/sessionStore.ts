import { create } from 'zustand';

interface SessionStore {
    sessionId: string | null;
    setSessionId: (id: string) => void;
}

export const useSessionStore = create<SessionStore>((set) => ({
    sessionId: null,
    setSessionId: (id) => set({ sessionId: id }),
}));
