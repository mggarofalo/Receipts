import { useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import client from "@/lib/api-client";
import type { components } from "@/generated/api";

type EnumValuePair = components["schemas"]["EnumValuePair"];
type EnumMetadataResponse = components["schemas"]["EnumMetadataResponse"];

function toLabelMap(pairs: EnumValuePair[]): Record<string, string> {
  const map: Record<string, string> = {};
  for (const p of pairs) {
    map[p.value] = p.label;
  }
  return map;
}

export function useEnumMetadata() {
  const query = useQuery({
    queryKey: ["metadata", "enums"],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/metadata/enums");
      if (error) throw error;
      return data as EnumMetadataResponse;
    },
    staleTime: Infinity,
    gcTime: Infinity,
  });

  const adjustmentTypes = useMemo(
    () => query.data?.adjustmentTypes ?? [],
    [query.data?.adjustmentTypes],
  );

  const authEventTypes = useMemo(
    () => query.data?.authEventTypes ?? [],
    [query.data?.authEventTypes],
  );

  const pricingModes = useMemo(
    () => query.data?.pricingModes ?? [],
    [query.data?.pricingModes],
  );

  const auditActions = useMemo(
    () => query.data?.auditActions ?? [],
    [query.data?.auditActions],
  );

  const entityTypes = useMemo(
    () => query.data?.entityTypes ?? [],
    [query.data?.entityTypes],
  );

  const adjustmentTypeLabels = useMemo(
    () => toLabelMap(adjustmentTypes),
    [adjustmentTypes],
  );

  const authEventLabels = useMemo(
    () => toLabelMap(authEventTypes),
    [authEventTypes],
  );

  const pricingModeLabels = useMemo(
    () => toLabelMap(pricingModes),
    [pricingModes],
  );

  const auditActionLabels = useMemo(
    () => toLabelMap(auditActions),
    [auditActions],
  );

  const entityTypeLabels = useMemo(
    () => toLabelMap(entityTypes),
    [entityTypes],
  );

  return useMemo(
    () => ({
      adjustmentTypes,
      authEventTypes,
      pricingModes,
      auditActions,
      entityTypes,
      adjustmentTypeLabels,
      authEventLabels,
      pricingModeLabels,
      auditActionLabels,
      entityTypeLabels,
      isLoading: query.isLoading,
    }),
    [
      adjustmentTypes,
      authEventTypes,
      pricingModes,
      auditActions,
      entityTypes,
      adjustmentTypeLabels,
      authEventLabels,
      pricingModeLabels,
      auditActionLabels,
      entityTypeLabels,
      query.isLoading,
    ],
  );
}
