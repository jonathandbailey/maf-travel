import { Collapse, Timeline, Spin } from "antd";
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
                    {update.details && (
                        <div style={{ color: "#555", fontSize: "0.75rem", marginTop: 4, fontStyle: "italic" }}>{update.details}</div>
                    )}
                </div>
            ),
        };
    });

    return (
        <Collapse
            defaultActiveKey={["1"]}
            size="small"
            style={{ alignSelf: "flex-start", minWidth: 280, maxWidth: 480 }}
            items={[{
                key: "1",
                label: "Thought process",
                children: <Timeline items={items} />,
            }]}
        />
    );
};

export default ThoughtProcess;
