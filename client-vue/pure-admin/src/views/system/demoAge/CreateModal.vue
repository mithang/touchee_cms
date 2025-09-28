<script lang="ts" setup>
import { reactive, ref } from "vue";
import { submitData, getSingle } from "@/api/system/demoAge";
import { VxeForm, VxeModal, VxeUI } from "vxe-pc-ui";

const emit = defineEmits(["success"]);

const modalRef = ref();
const formRef = ref();
const loading = ref(false);
const isEdit = ref(false);

const formData = reactive({
  id: null,
  name: "",
  age: null,
  birthday: null
});

const formItems = [
  {
    field: "name",
    title: "姓名",
    span: 24,
    itemRender: {
      name: "$input",
      props: { placeholder: "请输入姓名" }
    },
    titleWidth: 80
  },
  {
    field: "age",
    title: "年龄",
    span: 24,
    itemRender: {
      name: "$input",
      props: { type: "number", placeholder: "请输入年龄" }
    },
    titleWidth: 80
  },
  {
    field: "birthday",
    title: "生日",
    span: 24,
    itemRender: {
      name: "$input",
      props: { type: "date", placeholder: "请选择生日" }
    },
    titleWidth: 80
  }
];

const resetForm = () => {
  Object.assign(formData, {
    id: null,
    name: "",
    age: null,
    birthday: null
  });
};

const showAddModal = () => {
  isEdit.value = false;
  resetForm();
  modalRef.value.open();
};

const showEditModal = async (record: Recordable) => {
  isEdit.value = true;
  resetForm();
  
  try {
    const response = await getSingle(record.id);
    Object.assign(formData, response.data);
    modalRef.value.open();
  } catch (error) {
    VxeUI.modal.message({ content: "获取数据失败", status: "error" });
  }
};

const handleSubmit = async () => {
  const valid = await formRef.value.validate();
  if (!valid) return;
  
  loading.value = true;
  try {
    await submitData(formData);
    VxeUI.modal.message({ 
      content: `${isEdit.value ? '编辑' : '新增'}成功`, 
      status: "success" 
    });
    modalRef.value.close();
    emit("success");
  } catch (error) {
    VxeUI.modal.message({ 
      content: `${isEdit.value ? '编辑' : '新增'}失败`, 
      status: "error" 
    });
  } finally {
    loading.value = false;
  }
};

defineExpose({
  showAddModal,
  showEditModal
});
</script>

<template>
  <VxeModal
    ref="modalRef"
    :title="isEdit ? '编辑年龄信息' : '新增年龄信息'"
    width="500px"
    :mask-closable="false"
    :esc-closable="false"
  >
    <template #default>
      <VxeForm
        ref="formRef"
        :data="formData"
        :items="formItems"
        :rules="{
          name: [{ required: true, message: '请输入姓名' }],
          age: [{ required: true, message: '请输入年龄' }]
        }"
      />
    </template>
    <template #footer>
      <VxeButton content="取消" @click="modalRef.close()" />
      <VxeButton
        content="确定"
        status="primary"
        :loading="loading"
        @click="handleSubmit"
      />
    </template>
  </VxeModal>
</template>