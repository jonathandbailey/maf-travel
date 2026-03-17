import { Component, type ErrorInfo, type ReactNode } from "react";
import { Button } from "antd";

interface Props {
    children: ReactNode;
}

interface State {
    error: Error | null;
}

class ErrorBoundary extends Component<Props, State> {
    state: State = { error: null };

    static getDerivedStateFromError(error: Error): State {
        return { error };
    }

    componentDidCatch(error: Error, info: ErrorInfo) {
        console.error("Uncaught error:", error, info.componentStack);
    }

    render() {
        if (this.state.error) {
            return (
                <div role="alert" style={{ padding: 40, textAlign: "center" }}>
                    <h2>Something went wrong</h2>
                    <p style={{ color: "var(--color-text-muted)" }}>{this.state.error.message}</p>
                    <Button type="primary" onClick={() => this.setState({ error: null })}>
                        Try again
                    </Button>
                </div>
            );
        }
        return this.props.children;
    }
}

export default ErrorBoundary;
