<script lang="ts" setup>
import { ref, nextTick, reactive, h } from "vue";
import { VxeModalInstance, VxeFormInstance, VxeFormPropTypes } from "vxe-pc-ui";
import { getSingle, submitData } from "@/api/system/demo";
const emits = defineEmits<{ (e: "reload"): void }>();
const vxeModalRef = ref<VxeModalInstance>();
const modalOptions = reactive<{
  modalValue: boolean;
  modalTitle: string;
  canSubmit: boolean;
}>({
  modalValue: false,
  modalTitle: "",
  canSubmit: true
});

const showModal = (title: string, canSubmit?: boolean): void => {
  modalOptions.modalTitle = title;
  modalOptions.modalValue = true;
  modalOptions.canSubmit = canSubmit ?? true;
};

interface DemoInput {
  id: number;
  name: string;
}
const formRef = ref<VxeFormInstance>();
const defaultFormData = () => {
  return {
    id: null,
    name: ""
  };
};
const formData = ref<DemoInput>(defaultFormData());
const formItems = ref<VxeFormPropTypes.Items>([
  {
    field: "name",
    title: "Name",
    span: 24,
    itemRender: {
      name: "$input",
      props: { placeholder: "Please enter Name" }
    }
  },
]);
const formRules = ref<VxeFormPropTypes.Rules>({
});

const showAddModal = () => {
  showModal(`Add Demo`);
  formData.value = defaultFormData();
  nextTick(() => {
    formRef.value.clearValidate();
  });
};
const showEditModal = (record: Recordable) => {
  showModal(`Edit Demo->${record.name || record.id}`);
  nextTick(async () => {
    formRef.value.clearValidate();
    getSingle(record.id).then((data: any) => {
      formData.value = data;
    });
  });
};
const showViewModal = (record: Recordable) => {
  showModal(`View Demo->${record.name || record.id}`, false);
  nextTick(async () => {
    formRef.value.clearValidate();
    getSingle(record.id).then((data: any) => {
      formData.value = data;
    });
  });
};
const handleSubmit = async () => {
  const validate = await formRef.value.validate();
  if (!validate) {
    submitData(formData.value).then(() => {
      modalOptions.modalValue = false;
      emits("reload");
    });
  }
};

defineExpose({ showAddModal, showEditModal, showViewModal });
</script>
<template>
  <vxe-modal
    ref="vxeModalRef"
    v-model="modalOptions.modalValue"
    width="600"
    height="400"
    showFooter
    :destroy-on-close="true"
    :title="modalOptions.modalTitle"
  >
    <template #default>
      <vxe-form
        ref="formRef"
        :data="formData"
        :items="formItems"
        :rules="formRules"
        :titleWidth="100"
        :titleColon="true"
        :titleAlign="`right`"
      />
    </template>
    <template #footer>
      <vxe-button content="Close" @click="modalOptions.modalValue = false" />
      <vxe-button
        v-if="modalOptions.canSubmit"
        status="primary"
        content="Confirm"
        @click="handleSubmit"
      />
    </template>
  </vxe-modal>
</template>
