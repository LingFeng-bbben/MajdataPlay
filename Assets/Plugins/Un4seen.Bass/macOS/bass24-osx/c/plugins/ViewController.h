/*
	BASS plugins example
	Copyright (c) 2005-2021 Un4seen Developments Ltd.
*/

#import <Cocoa/Cocoa.h>

@interface ViewController : NSViewController
@property (strong) IBOutlet NSArrayController *pluginListController;
@property (weak) IBOutlet NSTextField *infoText;
@property (weak) IBOutlet NSSlider *positionSlider;
@property (weak) IBOutlet NSTextField *positionText;

@end

