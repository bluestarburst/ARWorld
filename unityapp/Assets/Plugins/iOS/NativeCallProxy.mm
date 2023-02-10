#import "NativeCallProxy.h"
#import <Foundation/Foundation.h>

@implementation FrameworkLibAPI

id<NativeCallsProtocol> api = NULL;
+ (void)registerAPIforNativeCalls:(id<NativeCallsProtocol>)aApi {
  api = aApi;
}

@end

/**
 * The methods below bridge the calls from Unity into iOS. When Unity call any
 * of the methods below, the call is forwarded to the iOS bridge using the
 * `NativeCallsProtocol`.
 */
extern "C" {

void sendUnityStateUpdate(const char *state) {
  const NSString *str = @(state);
  [api onUnityStateChange:str];
}

void setTestDelegate(TestDelegate delegate) {
  [api onSetTestDelegate:delegate];
}

void mapStatus(const char *status) {
  const NSString *str = @(status);
  [api onMapStatus:str];
}

void addingObj(const char *status) {
  const NSString *str = @(status);
  [api onAddingObj:str];
}

void ElementOptions(const char *type, const char *id, const char *chunkId, const char *storageId, const char *user, const char *createdBy) {
  const NSString *strType = @(type);
  const NSString *strId = @(id);
  const NSString *strChunkId = @(chunkId);
  const NSString *strStorageId = @(storageId);
  const NSString *strUser = @(user);
  const NSString *strCreatedBy = @(createdBy);
  [api onElementOptions:strType :strId :strChunkId :strStorageId :strUser :strCreatedBy];
}

// with array of strings
void MapList(const char *mapList) {
  const NSString *str = @(mapList);
  [api onMapList:str];
}

void SetPersistentDataPath(const char *path) {
  const NSString *str = @(path);
  [api onSetPersistentDataPath:str];
}

void LoadingMap(const char *mapID) {
  const NSString *str = @(mapID);
  [api onLoadingMap:str];
}

void Screenshot(const char *status) {
  const NSString *str = @(status);
  [api onScreenshot:str];
}

// this method will take a byte array in C# and convert it to a NSData object
// void sendUnityDataUpdate(const char *data, int length) {
//   const NSData *nsData = [NSData dataWithBytes:data length:length];
//   [api onSaveARWorldMap:nsData];
// }
}
