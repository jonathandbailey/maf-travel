export interface StatusUpdate {
    type: string;
    source: string;
    status: string;
    details: string;
}

export interface StatusUpdateSnapshot {
    snapshot: {
        type: string;
        payload: StatusUpdate;
    };
    type: string;
}