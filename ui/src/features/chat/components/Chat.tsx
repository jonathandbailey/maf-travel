import { Flex } from "antd";
import AgentFeedback from "./AgentFeedback";
import ChatInput from "./ChatInput";
import { useState } from "react";
import type { Exchange } from "../domain/Exchange";
import { ChatService } from "../api/chat.api";
import { UIFactory } from "../factories/UIFactory";
import type { Status } from "../domain/Status";
import { useStatusUpdateHandler } from "../hooks/useStatusUpdateHandler";
import { useChatResponseHandler } from "../hooks/useChatResponseHandler";

interface ChatProps {
    sessionId: string;
}

const Chat = ({ sessionId }: ChatProps) => {

    const [activeExchange, setActiveExchange] = useState<Exchange | null>(null);
    const [activeStatus, setActiveStatus] = useState<Status | null>(null);

    useStatusUpdateHandler({ setActiveStatus, setActiveExchange });
    useChatResponseHandler({ setActiveExchange });


    function handlePrompt(value: string): void {
        const newExchange = UIFactory.createUIExchange(value);

        setActiveExchange(newExchange);

        const conversationService = new ChatService();
        conversationService.startChatExchange(
            value,
            newExchange.user.id,
            sessionId,
            newExchange.assistant.id
        ).then(() => {
            // Conversation exchange initiated successfully
        }).catch(error => {
            console.error("Error during conversation exchange:", error);
        });
    }


    return (<>
        <Flex vertical>
            {activeExchange && <AgentFeedback message={activeExchange} status={activeStatus} />}

            <ChatInput onEnter={handlePrompt} />
        </Flex>
    </>)
};
export default Chat;