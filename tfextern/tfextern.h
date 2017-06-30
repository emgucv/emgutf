#include "tensorflow/c/c_api.h"
#include "tensorflow/core/util/port.h"

#ifndef  TFAPI_EXPORTS
#define TFAPI_EXPORTS
#endif

#if (defined WIN32 || defined _WIN32 || defined WINCE || defined __CYGWIN__) && defined TFAPI_EXPORTS
#  define TF_EXPORTS __declspec(dllexport)
#elif defined __GNUC__ && __GNUC__ >= 4
#  define TF_EXPORTS __attribute__ ((visibility ("default")))
#else
#  define TF_EXPORTS
#endif

#ifndef TF_EXTERN_C
#  ifdef __cplusplus
#    define TF_EXTERN_C extern "C"
#  else
#    define TF_EXTERN_C
#  endif
#endif

#if defined WIN32 || defined _WIN32
#  define TF_CDECL __cdecl
#  define TF_STDCALL __stdcall
#else
#  define TF_CDECL
#  define TF_STDCALL
#endif

#ifndef TFAPI
#  define TFAPI(rettype) TF_EXTERN_C TF_EXPORTS rettype TF_CDECL
#endif

//#include <stddef.h>
//#include <stdint.h>
#include <cstdlib>
#include <cstring>

TFAPI(const char*) tfeGetVersion();

TFAPI(TF_Tensor*) tfeAllocateTensor(int tfDataType, int* dims,	int numDims, int len);
TFAPI(void) tfeDeleteTensor(TF_Tensor** tensor);
TFAPI(void*) tfeTensorData(TF_Tensor* tensor);
TFAPI(int) tfeTensorByteSize(TF_Tensor* tensor);
TFAPI(TF_DataType) tfeTensorType(TF_Tensor* tensor);
TFAPI(int) tfeNumDims(TF_Tensor* tensor);
TFAPI(void) tfeGetDim(TF_Tensor* tensor, int* dims, int numDims);

TFAPI(TF_SessionOptions*) tfeNewSessionOptions();
TFAPI(void) tfeDeleteSessionOptions(TF_SessionOptions** session_options);
TFAPI(void) tfeCloseSession(TF_Session* session, TF_Status* status);
TFAPI(void) tfeSetTarget(TF_SessionOptions* options, const char* target);

TFAPI(TF_Session*) tfeNewSession(TF_Graph* graph, const TF_SessionOptions* opts, TF_Status* status);
TFAPI(void) tfeDeleteSession(TF_Session** session, TF_Status* status);
TFAPI(void) tfeSessionRun(
	TF_Session* session, const TF_Buffer* run_options,
	TF_Operation** inputOps, int* inputIdx, TF_Tensor* const* input_values,	int ninputs, 
	TF_Operation** outputOps, int* outputIdx, TF_Tensor** output_values, int noutputs,
	const TF_Operation* const* target_opers, int ntargets,
	TF_Buffer* run_metadata, TF_Status* status);

TFAPI(TF_Status*) tfeNewStatus();
TFAPI(void) tfeDeleteStatus(TF_Status** status);
TFAPI(int) tfeGetCode(TF_Status* s);
TFAPI(const char*) tfeMessage(TF_Status* s);

TFAPI(TF_ImportGraphDefOptions*) tfeNewImportGraphDefOptions();
TFAPI(void) tfeDeleteImportGraphDefOptions(TF_ImportGraphDefOptions** opts);
TFAPI(void) tfeImportGraphDefOptionsSetPrefix(TF_ImportGraphDefOptions* opts, const char* prefix);
TFAPI(void) tfeImportGraphDefOptionsAddInputMapping(
	TF_ImportGraphDefOptions* opts, 
	const char* src_name, 
	int src_index,
	TF_Operation* dstOp,
	int dstOpIdx);
TFAPI(void) tfeImportGraphDefOptionsRemapControlDependency(
	TF_ImportGraphDefOptions* opts, 
	const char* src_name, 
	TF_Operation* dst);
TFAPI(void) tfeImportGraphDefOptionsAddControlDependency(
	TF_ImportGraphDefOptions* opts, TF_Operation* oper);
TFAPI(void) tfeImportGraphDefOptionsAddReturnOutput(
	TF_ImportGraphDefOptions* opts, const char* oper_name, int index);
TFAPI(int) tfeImportGraphDefOptionsNumReturnOutputs(TF_ImportGraphDefOptions* opts);

TFAPI(TF_Graph*) tfeNewGraph();
TFAPI(void) tfeDeleteGraph(TF_Graph** graph);
TFAPI(void) tfeGraphImportGraphDef(TF_Graph* graph, const TF_Buffer* graph_def,	const TF_ImportGraphDefOptions* options,TF_Status* status);
TFAPI(TF_Operation*) tfeGraphOperationByName(TF_Graph* graph, const char* oper_name);
TFAPI(TF_Operation*) tfeGraphNextOperation(TF_Graph* graph, size_t* pos);
TFAPI(void) tfeGraphToGraphDef(TF_Graph* graph, TF_Buffer* output_graph_def, TF_Status* status);
TFAPI(void) tfeGraphSetTensorShape(TF_Graph* graph, TF_Operation* outputOperation, int idx, const int* dims, const int num_dims, TF_Status* status);
TFAPI(void) tfeGraphGetTensorShape(TF_Graph* graph, TF_Operation* outputOperation, int idx,	int* dims, int num_dims, TF_Status* status);

TFAPI(TF_OperationDescription*) tfeNewOperation(TF_Graph* graph, char* op_type, char* oper_name);
TFAPI(TF_Operation*) tfeFinishOperation(TF_OperationDescription* desc, TF_Status* status);
TFAPI(const char*) tfeOperationName(TF_Operation* oper);
TFAPI(const char*) tfeOperationOpType(TF_Operation* oper);
TFAPI(const char*) tfeOperationDevice(TF_Operation* oper);
TFAPI(int) tfeOperationNumOutputs(TF_Operation* oper);
TFAPI(TF_DataType) tfeOperationOutputType(TF_Operation* oper, int idx);
TFAPI(int) tfeOperationNumInputs(TF_Operation* oper);
TFAPI(TF_DataType) tfeOperationInputType(TF_Operation* oper, int idx);

TFAPI(void) tfeAddInput(TF_OperationDescription* desc, TF_Operation* oper, int index);
TFAPI(void) tfeSetAttrInt(TF_OperationDescription* desc, const char* attr_name,	int64_t value);
TFAPI(void) tfeSetAttrIntList(TF_OperationDescription* desc, const char* attr_name, const int64_t* values, int num_values);
TFAPI(void) tfeSetAttrBool(TF_OperationDescription* desc, const char* attr_name, bool value);
TFAPI(void) tfeSetAttrBoolList(TF_OperationDescription* desc, const char* attr_name, const unsigned char* values, int num_values);
TFAPI(void) tfeSetAttrFloat(TF_OperationDescription* desc, const char* attr_name, float value);
TFAPI(void) tfeSetAttrFloatList(TF_OperationDescription* desc, const char* attr_name, const float* values, int num_values);
TFAPI(void) tfeSetAttrString(TF_OperationDescription* desc,	const char* attr_name, const void* value, int length);
TFAPI(void) tfeSetAttrType(TF_OperationDescription* desc, const char* attr_name, TF_DataType value);
TFAPI(void) tfeSetAttrTypeList(TF_OperationDescription* desc, const char* attr_name, const TF_DataType* values,	int num_values);
TFAPI(void) tfeSetAttrShape(TF_OperationDescription* desc, const char* attr_name, const int64_t* dims, int num_dims);
TFAPI(void) tfeSetAttrShapeList(TF_OperationDescription* desc, const char* attr_name, const int64_t* const* dims, const int* num_dims, int num_shapes);
TFAPI(void) tfeSetAttrTensor(TF_OperationDescription* desc,	const char* attr_name, TF_Tensor* value, TF_Status* status);
TFAPI(void) tfeSetDevice(TF_OperationDescription* desc, const char* device);
TFAPI(void) tfeAddInputList(TF_OperationDescription* desc, TF_Operation** inputOps, int* indices, int num_inputs);
TFAPI(void) tfeAddControlInput(TF_OperationDescription* desc, TF_Operation* input);
TFAPI(void) tfeColocateWith(TF_OperationDescription* desc, TF_Operation* op);

TFAPI(TF_Buffer*) tfeNewBuffer();
TFAPI(TF_Buffer*) tfeNewBufferFromString(const void* proto, int proto_len);
TFAPI(void) tfeDeleteBuffer(TF_Buffer** buffer);
TFAPI(const void*) tfeBufferGetData(TF_Buffer* buffer);
TFAPI(int) tfeBufferGetLength(TF_Buffer* buffer);


TFAPI(int) tfeDataTypeSize(TF_DataType dt);

TFAPI(TF_Buffer*) tfeGetAllOpList();

TFAPI(int) tfeStringEncodedSize(int len);
TFAPI(int) tfeStringEncode(const char* src, int src_len, char* dst, int dst_len, TF_Status* status);
TFAPI(int) tfeStringDecode(const char* src, int src_len, const char** dst, size_t* dst_len, TF_Status* status);
TFAPI(void) tfeMemcpy(void* dst, void* src, int length);

TFAPI(bool) tfeIsGoogleCudaEnabled();