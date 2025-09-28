<script lang="ts" setup>
import { h, reactive, ref, onMounted } from "vue";
import { getPageList, deleteData } from "@/api/system/demoAge";
import { ReVxeGrid } from "@/components/ReVxeTable";
import { VxeButton, VxeUI } from "vxe-pc-ui";
import CreateModal from "./CreateModal.vue";

const reVxeGridRef = ref();
const columns = [
  { type: "checkbox", title: "", width: 60, align: "center" },
  {
    title: "Id",
    field: "id",
    minWidth: 100
  },
  {
    title: "姓名",
    field: "name",
    minWidth: 150
  },
  {
    title: "年龄",
    field: "age",
    minWidth: 100
  },
  {
    title: "生日",
    field: "birthday",
    minWidth: 150,
    formatter: ({ cellValue }) => {
      return cellValue ? new Date(cellValue).toLocaleDateString() : "";
    }
  }
];

const formRef = ref();

const handleInitialFormParams = () => ({
  name: ""
});

const formItems = [
  {
    field: "name",
    title: "姓名",
    span: 6,
    itemRender: { name: "$input", props: { placeholder: "姓名" } }
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
            content: "查询",
            status: "primary"
          }
        },
        { props: { type: "reset", icon: "vxe-icon-undo", content: "重置" } }
      ]
    }
  }
];

const formData = reactive<{ name: string }>(handleInitialFormParams());

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
  const type = await VxeUI.modal.confirm("您确定要删除吗？");
  if (type == "confirm") {
    deleteData(record.id).then(() => {
      handleSearch();
    });
  }
};

const handleView = (record: Recordable) => {
  createModalRef.value.showViewModal(record);
};

// Thêm định nghĩa functions cho ReVxeGrid
const functions = ref({
  add: "system.demoage.add",
  edit: "system.demoage.edit",
  view: "system.demoage.view",
  delete: "system.demoage.delete"
});

// Thêm để debug
onMounted(() => {
  console.log("DemoAge component mounted");
  console.log("API base URL:", import.meta.env.VITE_API_BASE_URL);
});
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
    <CreateModal ref="createModalRef" @success="handleSearch" />
  </div>
</template>
