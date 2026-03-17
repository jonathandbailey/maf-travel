import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import ChatInput from './ChatInput';

function renderChatInput(overrides: Partial<React.ComponentProps<typeof ChatInput>> = {}) {
    const props = {
        value: '',
        onChange: vi.fn(),
        onKeyDown: vi.fn(),
        ...overrides,
    };
    return { ...render(<ChatInput {...props} />), props };
}

describe('ChatInput', () => {
    it('renders the message input with placeholder', () => {
        renderChatInput();

        expect(screen.getByPlaceholderText('Ask me anything...')).toBeInTheDocument();
    });

    it('submit button is disabled when value is empty and not streaming', () => {
        renderChatInput({ value: '' });

        expect(screen.getByRole('button', { name: 'Send message' })).toBeDisabled();
    });

    it('submit button is enabled when value has content', () => {
        renderChatInput({ value: 'Hello' });

        expect(screen.getByRole('button', { name: 'Send message' })).not.toBeDisabled();
    });

    it('shows stop button when streaming', () => {
        renderChatInput({ isStreaming: true, value: '' });

        expect(screen.getByRole('button', { name: 'Stop streaming' })).toBeInTheDocument();
    });

    it('calls onSubmit when send button is clicked', async () => {
        const onSubmit = vi.fn();
        renderChatInput({ value: 'Hello', onSubmit });

        await userEvent.click(screen.getByRole('button', { name: 'Send message' }));

        expect(onSubmit).toHaveBeenCalledOnce();
    });

    it('calls onCancel when stop button is clicked during streaming', async () => {
        const onCancel = vi.fn();
        renderChatInput({ isStreaming: true, onCancel });

        await userEvent.click(screen.getByRole('button', { name: 'Stop streaming' }));

        expect(onCancel).toHaveBeenCalledOnce();
    });

    it('calls onChange when user types', async () => {
        const onChange = vi.fn();
        renderChatInput({ onChange });

        await userEvent.type(screen.getByPlaceholderText('Ask me anything...'), 'Hi');

        expect(onChange).toHaveBeenCalled();
    });

    it('suggestion dropdown button is present', () => {
        renderChatInput();

        expect(screen.getByRole('button', { name: 'Choose a suggestion' })).toBeInTheDocument();
    });
});
