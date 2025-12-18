import { Divider, Flex, Tabs } from "antd";
import UserMessage from "./UserMessage";
import AssistantMessage from "./AssistantMessage";
import AgentStatus from "./AgentStatus";
import { useExchangesStore } from "../stores/exchanges.store";


const ChatOutput = () => {

    const { exchanges } = useExchangesStore();

    return (
        <>
            <div >
                <Tabs type="card"
                    style={{ height: '100%' }}
                    items={[
                        {
                            label: 'Chat',
                            key: 'chat',
                            children: (
                                <div>
                                    {exchanges.map((exchange, idx) => (
                                        <div key={idx}>
                                            <Flex justify="flex-end" >
                                                <UserMessage message={exchange.user} />
                                            </Flex>
                                            <AssistantMessage message={exchange.assistant} />
                                            <Divider />
                                            <AgentStatus statusItems={exchange.status || []} />
                                            <Divider />
                                        </div>
                                    ))}
                                </div>
                            )
                        }
                    ]}
                />
            </div>
        </>);
}

export default ChatOutput;