import { useEffect } from "react";
import type { ChatResponseDto } from "../types/dto/chat-response.dto";
import type { UIExchange } from "../types/ui/UIExchange";
import streamingService from "../services/streaming.service";

interface UseExchangeStatusUpdateHandlerProps {
    setExchanges: React.Dispatch<React.SetStateAction<UIExchange[]>>;
}

export const useExchangeStatusUpdateHandler = ({ setExchanges }: UseExchangeStatusUpdateHandlerProps) => {
    useEffect(() => {

        const handleExchangeStatusUpdate = (response: ChatResponseDto) => {
            setExchanges(prev =>
                prev.map(exchange => {
                    if (exchange.assistant.id === response.id) {
                        return {
                            ...exchange,
                            assistant: {
                                ...exchange.assistant,
                                statusMessage: response.message || ''
                            }
                        };
                    }
                    return exchange;
                })
            );
        };

        streamingService.on("status", handleExchangeStatusUpdate);

        return () => {

            streamingService.off("status", handleExchangeStatusUpdate);
        };
    }, [setExchanges]);
};