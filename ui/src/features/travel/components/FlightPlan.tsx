import { useState } from "react";
import { Drawer, Tag, theme } from "antd";
import { useFlightSearchStore } from "../store/flightSearchStore";
import type { FlightSearchResponse } from "../services/flightSearchService";
import FlightSearch from "./FlightSearch";

const { useToken } = theme;

const pillLabel = (search: FlightSearchResponse): string => {
    const first = search.flightOptions[0];
    return first ? `${first.origin} → ${first.destination}` : "Flight Search";
};

const FlightPlan = () => {
    const flightSearches = useFlightSearchStore((s) => s.flightSearches);
    const { token } = useToken();
    const [selectedSearch, setSelectedSearch] = useState<FlightSearchResponse | null>(null);
    const [drawerOpen, setDrawerOpen] = useState(false);

    if (flightSearches.length === 0) return null;

    const handlePillClick = (search: FlightSearchResponse) => {
        setSelectedSearch(search);
        setDrawerOpen(true);
    };

    return (
        <>
            <div style={{ display: "flex", flexWrap: "wrap", gap: token.marginXS, padding: `${token.paddingXS}px 0` }}>
                {flightSearches.map((search) => (
                    <Tag
                        key={search.id}
                        color="blue"
                        style={{ cursor: "pointer" }}
                        onClick={() => handlePillClick(search)}
                    >
                        {pillLabel(search)}
                    </Tag>
                ))}
            </div>
            <Drawer
                title="Flight Options"
                placement="right"
                open={drawerOpen}
                onClose={() => setDrawerOpen(false)}
                width={480}
            >
                {selectedSearch && <FlightSearch flightSearch={selectedSearch} />}
            </Drawer>
        </>
    );
};

export default FlightPlan;
