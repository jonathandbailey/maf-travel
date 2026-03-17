import { describe, it, expect, vi, beforeEach, type Mock } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { EventType } from '@ag-ui/client';
import type { BaseEvent, TextMessageContentEvent } from '@ag-ui/client';
import { useChatAgent } from './useChatAgent';
import type { ChatAgentCallbacks } from '../services/ChatAgentClient';

// Capture the callbacks passed to the ChatAgentClient constructor
let capturedCallbacks: ChatAgentCallbacks;
let mockSendMessage: Mock;
let mockCancel: Mock;

vi.mock('../services/ChatAgentClient', () => ({
    ChatAgentClient: class {
        sendMessage: Mock;
        cancel: Mock;
        setThreadId: Mock;
        addEventHandler: Mock;
        removeEventHandler: Mock;

        constructor(_url: string, _threadId: string, callbacks: ChatAgentCallbacks) {
            capturedCallbacks = callbacks;
            mockSendMessage = vi.fn();
            mockCancel = vi.fn();
            this.sendMessage = mockSendMessage;
            this.cancel = mockCancel;
            this.setThreadId = vi.fn();
            this.addEventHandler = vi.fn();
            this.removeEventHandler = vi.fn();
        }
    },
}));

vi.mock('@/app/store/sessionStore', () => ({
    useSessionStore: Object.assign(
        (selector: (s: { sessionId: string | null }) => unknown) => selector({ sessionId: null }),
        {
            getState: () => ({ sessionId: null }),
            subscribe: vi.fn(() => () => {}),
        }
    ),
}));

vi.mock('../../travel/store/travelPlanStore', () => ({
    useTravelPlanStore: Object.assign(
        (selector: (s: { planVersion: number }) => unknown) => selector({ planVersion: 0 }),
        {
            subscribe: vi.fn(() => () => {}),
        }
    ),
}));

beforeEach(() => {
    vi.clearAllMocks();
});

describe('useChatAgent', () => {
    it('starts with empty exchanges and not streaming', () => {
        const { result } = renderHook(() => useChatAgent());

        expect(result.current.exchanges).toEqual([]);
        expect(result.current.isStreaming).toBe(false);
    });

    it('delegates sendMessage to the client', () => {
        const { result } = renderHook(() => useChatAgent());

        act(() => {
            result.current.sendMessage('Hello');
        });

        expect(mockSendMessage).toHaveBeenCalledWith('Hello');
    });

    it('sets isStreaming to true and adds exchange when run starts', () => {
        const { result } = renderHook(() => useChatAgent());

        act(() => {
            capturedCallbacks.onRunStarted('exchange-1', 'Hello');
        });

        expect(result.current.isStreaming).toBe(true);
        expect(result.current.exchanges[0].userContent).toBe('Hello');
        expect(result.current.exchanges[0].id).toBe('exchange-1');
    });

    it('accumulates assistant content from TEXT_MESSAGE_CONTENT events', () => {
        const { result } = renderHook(() => useChatAgent());

        act(() => {
            capturedCallbacks.onRunStarted('exchange-1', 'Hello');
        });

        const makeTextEvent = (delta: string): BaseEvent =>
            ({ type: EventType.TEXT_MESSAGE_CONTENT, delta } as TextMessageContentEvent);

        act(() => {
            capturedCallbacks.onEvent(makeTextEvent('Hello'), 'exchange-1');
            capturedCallbacks.onEvent(makeTextEvent(' world'), 'exchange-1');
        });

        expect(result.current.exchanges[0].assistantContent).toBe('Hello world');
    });

    it('sets isStreaming to false when run completes', () => {
        const { result } = renderHook(() => useChatAgent());

        act(() => {
            capturedCallbacks.onRunStarted('exchange-1', 'Hello');
        });
        act(() => {
            capturedCallbacks.onRunCompleted('exchange-1');
        });

        expect(result.current.isStreaming).toBe(false);
    });

    it('sets error on exchange when run fails', () => {
        const { result } = renderHook(() => useChatAgent());

        act(() => {
            capturedCallbacks.onRunStarted('exchange-1', 'Hello');
        });
        act(() => {
            capturedCallbacks.onRunFailed('exchange-1', new Error('Connection lost'));
        });

        expect(result.current.exchanges[0].error).toBe('Connection lost');
    });

    it('delegates cancel to the client', () => {
        const { result } = renderHook(() => useChatAgent());

        act(() => {
            result.current.handleCancel();
        });

        expect(mockCancel).toHaveBeenCalledOnce();
    });
});
