import { Card, Timeline, Spin } from "antd";
import type { StatusUpdate } from "../domain/StatusUpdate";

interface ThoughtProcessProps {
    statusUpdates: StatusUpdate[];
    isLoading?: boolean;
}

const ThoughtProcess = ({ statusUpdates, isLoading }: ThoughtProcessProps) => {
    const items = statusUpdates.map((update, index) => {
        const isLast = index === statusUpdates.length - 1;
        return {
            dot: isLast && isLoading ? <Spin size="small" /> : undefined,
            children: (
                <div>
                    <div style={{ fontWeight: 500, fontSize: "0.875rem" }}>{update.status}</div>
                    {update.source && (
                        <div style={{ color: "#888", fontSize: "0.75rem" }}>{update.source}</div>
                    )}
                </div>
            ),
        };
    });

    return (
        <Card
            size="small"
            style={{ alignSelf: "flex-start", minWidth: 280, maxWidth: 480 }}
            styles={{ body: { padding: "12px 16px" } }}
        >
            <Timeline items={items} />
        </Card>
    );
};

export default ThoughtProcess;
