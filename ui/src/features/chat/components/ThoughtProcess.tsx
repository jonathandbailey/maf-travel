import { Collapse, Timeline, Spin } from "antd";
import type { StatusUpdate } from "../domain/StatusUpdate";
import "./chat.css";

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
                    <div className="thought-process-status">{update.status}</div>
                    {update.source && (
                        <div className="thought-process-source">{update.source}</div>
                    )}
                    {update.details && (
                        <div className="thought-process-details">{update.details}</div>
                    )}
                </div>
            ),
        };
    });

    return (
        <Collapse
            defaultActiveKey={["1"]}
            size="small"
            className="thought-process"
            items={[{
                key: "1",
                label: "Thought process",
                children: <Timeline items={items} />,
            }]}
        />
    );
};

export default ThoughtProcess;
