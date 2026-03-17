import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import Exchange from './Exchange';
import type { StatusUpdate } from '../domain/StatusUpdate';

const noStatusUpdates: StatusUpdate[] = [];

const statusUpdates: StatusUpdate[] = [
    { type: 'Planning', source: 'planner_agent', status: 'Analyzing request', details: '' },
];

describe('Exchange', () => {
    it('shows loading spinner when no content, no statusUpdates, and no error', () => {
        render(
            <Exchange
                userContent="Where should I go?"
                statusUpdates={noStatusUpdates}
            />
        );

        expect(screen.getByLabelText('Loading response')).toBeInTheDocument();
    });

    it('shows ThoughtProcess when statusUpdates are present', () => {
        render(
            <Exchange
                userContent="Plan my trip"
                statusUpdates={statusUpdates}
            />
        );

        expect(screen.getByText('Thought process')).toBeInTheDocument();
    });

    it('does not show loading spinner when statusUpdates are present', () => {
        render(
            <Exchange
                userContent="Plan my trip"
                statusUpdates={statusUpdates}
            />
        );

        expect(screen.queryByLabelText('Loading response')).not.toBeInTheDocument();
    });

    it('shows AssistantMessage when assistantContent is provided', () => {
        render(
            <Exchange
                userContent="Hello"
                assistantContent="Here is your plan."
                statusUpdates={noStatusUpdates}
            />
        );

        expect(screen.getByText('Here is your plan.')).toBeInTheDocument();
    });

    it('does not show loading spinner when assistantContent is present', () => {
        render(
            <Exchange
                userContent="Hello"
                assistantContent="Done."
                statusUpdates={noStatusUpdates}
            />
        );

        expect(screen.queryByLabelText('Loading response')).not.toBeInTheDocument();
    });

    it('shows ChatError when error is provided', () => {
        render(
            <Exchange
                userContent="Hello"
                statusUpdates={noStatusUpdates}
                error="Something went wrong"
            />
        );

        expect(screen.getByText('Something went wrong')).toBeInTheDocument();
    });

    it('renders user content', () => {
        render(
            <Exchange
                userContent="Take me to Rome"
                statusUpdates={noStatusUpdates}
            />
        );

        expect(screen.getByText('Take me to Rome')).toBeInTheDocument();
    });

    it('has article role for accessibility', () => {
        render(
            <Exchange
                userContent="Hello"
                statusUpdates={noStatusUpdates}
            />
        );

        expect(screen.getByRole('article')).toBeInTheDocument();
    });
});
