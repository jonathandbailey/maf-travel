import apiClient from "../../../api/client/api-client";
import type { ChatRequestDto } from "../../../types/dto/chat-request.dto";
import type { ChatResponseDto } from "../../../types/dto/chat-response.dto";


export class ConversationService {


    async startConversationExchange(message: string, id: string, sessionId: string, exchangeId: string): Promise<ChatResponseDto> {


        const request: ChatRequestDto = { message, id, sessionId, exchangeId };

        const response = await apiClient.post<ChatResponseDto>(`api/conversations`, request);
        return response.data;
    }


}