export interface ArtifactStatusDto {
    key: string;
}

export interface ChatRequestDto {
    id: string;
    message: string;
    sessionId: string;
    exchangeId: string;
}

export interface ChatResponseDto {
    id: string
    message: string;
    details: string;
    threadId: string;
    isEndOfStream: boolean;
    source: string;
}