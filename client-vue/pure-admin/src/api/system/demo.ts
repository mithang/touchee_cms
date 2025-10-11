import { http } from "@/utils/http";

// Get all demo records
export const getAllList = (params?: any) => {
  return http.request("get", "/demo/demos", params);
};

// Get paginated list of demo records
export function getPageList(params?: any) {
  return http.request("get", "/demo/paged-list", { params });
};

// Submit data (create or update)
export const submitData = (params: any) => {
  return http.request(
    params.id ? "put" : "post",
    `/demo/${params.id ?? ""}`,
    {
      data: params
    }
  );
};

// Get a single demo record by ID
export const getSingle = (id: number) => {
  return http.request("get", `/demo/${id}`);
};

// Delete a demo record by ID
export const deleteData = (id: number) => {
  return http.request("delete", `/demo/${id}`);
};

// Batch delete demo records
export const batchDelete = (ids: number[]) => {
  return http.request("delete", "/demo/batch", {
    data: { ids }
  });
};

// Export demo records
export const exportData = (params?: any) => {
  return http.request("get", "/demo/export", {
    params,
    responseType: "blob"
  });
};
