import { http } from "@/utils/http";

export const getAllList = params => {
  return http.request("get", "/demo-age/demo-ages", params);
};

export function getPageList(params) {
  return http.request("get", "/demo-age/paged-list", { params });
}

export const submitData = (params: any) => {
  return http.request(
    params.id ? "put" : "post",
    `/demo-age/${params.id ?? ""}`,
    {
      data: params
    }
  );
};

export const getSingle = (id: number) => {
  return http.request("get", `/demo-age/${id}`);
};

export const deleteData = (id: number) => {
  return http.request("delete", `/demo-age/${id}`);
};
