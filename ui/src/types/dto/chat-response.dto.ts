export interface ChatResponseDto {
    id: string
    message: string;
    details: string;
    threadId: string;
    isEndOfStream: boolean;
}