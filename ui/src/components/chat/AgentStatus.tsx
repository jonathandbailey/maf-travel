import { Flex, Timeline, Typography } from "antd";
import type { Status } from "../../types/ui/Status";

interface AgentStatusProps {
    statusItems: Status[];
}

const { Text } = Typography;

const AgentStatus = ({ statusItems }: AgentStatusProps) => {
    return (
        <>
            <div style={{ marginTop: "8px", marginBottom: "16px" }} >
                <Timeline>
                    {statusItems.map((status, index) => (
                        <Timeline.Item key={index} >
                            <Flex vertical>
                                {status.message}
                                <Text type="secondary" style={{ marginTop: "4px", marginBottom: "8px" }}>{status.details}</Text>

                            </Flex>

                        </Timeline.Item>
                    ))}
                </Timeline>
            </div>
        </>
    )
}

export default AgentStatus;