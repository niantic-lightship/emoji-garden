// Copyright 2023 Niantic, Inc. All Rights Reserved.
#import <Foundation/Foundation.h>
#import <AVFoundation/AVFoundation.h>
#import <CoreLocation/CoreLocation.h>

@interface PermissionCheck : NSObject
+ (int)LocationPermissionStatus;
+ (int)CameraPermissionStatus;
+ (void)RequestCameraPermission;
@end

@implementation PermissionCheck

+ (int)LocationPermissionStatus {
    CLAuthorizationStatus status = [CLLocationManager authorizationStatus];

    switch (status) {
    case kCLAuthorizationStatusNotDetermined:
        return 0;
    case kCLAuthorizationStatusRestricted:
        return 1;
    case kCLAuthorizationStatusDenied:
        return 2;
    case kCLAuthorizationStatusAuthorizedAlways:
        return 3;
    case kCLAuthorizationStatusAuthorizedWhenInUse:
        return 4;
    default:
        return -1;  // Unknown status
    }
}

+ (int)CameraPermissionStatus {
    AVAuthorizationStatus status = [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeVideo];
    
    switch (status) {
        case AVAuthorizationStatusNotDetermined:
            return 0;
        case AVAuthorizationStatusRestricted:
            return 1;
        case AVAuthorizationStatusDenied:
            return 2;
        case AVAuthorizationStatusAuthorized:
            return 3;
        default:
            return -1;  // Unknown status
    }
}

+ (void)RequestCameraPermission {
    [AVCaptureDevice requestAccessForMediaType:AVMediaTypeVideo completionHandler:^(BOOL granted) {
    }];
}

@end

extern "C" {
    int _CameraPermissionStatus() {
        return [PermissionCheck CameraPermissionStatus]; 
    }

   int _LocationPermissionStatus() {
        return [PermissionCheck LocationPermissionStatus];
    }

    void _RequestCameraPermission() {
        [PermissionCheck RequestCameraPermission];
    }
}
