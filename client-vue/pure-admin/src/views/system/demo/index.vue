<script lang="ts" setup>
import { h, reactive, ref } from "vue";
import { getPageList, deleteData } from "@/api/system/demo";
import { ReVxeGrid } from "@/components/ReVxeTable";
import { VxeButton, VxeUI } from "vxe-pc-ui";
import CreateModal from "./CreateModal.vue";

const reVxeGridRef = ref();
const columns = [
  { type: "checkbox", title: "", width: 60, align: "center" },
  {
    title: "Name",
    field: "name",
    minWidth: 150
  },
];
const formRef = ref();

const handleInitialFormParams = () => ({
  name: ""
});
const formItems = [
  {
    field: "name",
    title: "Name",
    span: 6,
    itemRender: { name: "$input", props: { placeholder: "Name" } }
  },
  {
    span: 6,
    itemRender: {
      name: "$buttons",
      children: [
        {
          props: {
            type: "submit",
            icon: "vxe-icon-search",
            content: "Search",
            status: "primary"
          }
        },
        { props: { type: "reset", icon: "vxe-icon-undo", content: "Reset" } }
      ]
    }
  }
];
const formData = reactive(handleInitialFormParams());

const handleSearch = () => {
  reVxeGridRef.value.loadData();
};

const createModalRef = ref();
const handleAdd = () => {
  createModalRef.value.showAddModal();
};
const handleEdit = (record: Recordable) => {
  createModalRef.value.showEditModal(record);
};
const handleDelete = async (record: Recordable) => {
  const type = await VxeUI.modal.confirm("Are you sure you want to delete this record?");
  if (type == "confirm") {
    deleteData(record.id).then(() => {
      handleSearch();
    });
  }
};
const handleView = (record: Recordable) => {
  createModalRef.value.showViewModal(record);
};
const functions: Record<string, string> = {
  add: "system.demo.add",
  edit: "system.demo.edit",
  view: "system.demo.view",
  delete: "system.demo.delete"
};
</script>
<template>
  <div>
    <el-card :shadow="`never`">
      <vxe-form
        ref="formRef"
        :data="formData"
        :items="formItems"
        @submit="handleSearch"
        @reset="handleInitialFormParams"
      />
    </el-card>
    <el-card :shadow="`never`" class="table-card">
      <ReVxeGrid
        ref="reVxeGridRef"
        :request="getPageList"
        :functions="functions"
        :searchParams="formData"
        :columns="columns"
        @handleAdd="handleAdd"
        @handleEdit="handleEdit"
        @handleDelete="handleDelete"
        @handleView="handleView"
      />
    </el-card>
    <CreateModal ref="createModalRef" @reload="handleSearch" />
  </div>
</template>
