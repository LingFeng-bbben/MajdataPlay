import window from '@ohos.window';
import { abilityAccessCtrl, Permissions, AbilityConstant, UIAbility, Want, common } from '@kit.AbilityKit';
import { SetToGlobalThis, TuanjiePlayer, TuanjieLog } from 'tuanjieLib';
import { BusinessError } from '@kit.BasicServicesKit';
import { Environment, fileIo as fs } from '@kit.CoreFileKit';
//import { launchAcceleration } from '@kit.GraphicsAccelerateKit';

const APP_DATA_SUBDIR: string = 'app'
const DOWNLOAD_PERMISSION: Permissions = 'ohos.permission.READ_WRITE_DOWNLOAD_DIRECTORY'

export default class TuanjiePlayerAbility extends UIAbility {
  private defaultPageUri: string = "pages/Index"
  private tuanjiePlayer : TuanjiePlayer = TuanjiePlayer.getInstance()
  onCreate(want: Want, launchParam: AbilityConstant.LaunchParam): void {
    this.setConfig();
    this.registerHmsAbility();
    this.tuanjiePlayer.onCreate(this.context, want, launchParam);
  }

  onDestroy(): void {
    this.tuanjiePlayer.onDestroy();
  }

  onWindowStageCreate(windowStage: window.WindowStage): void {
    // Request download permission before loading game content and file operations
    this.requestDownloadPermission().then((granted: boolean) => {
      if (granted) {
        this.setupAppDataDir();
      } else {
        TuanjieLog.error('Download directory permission denied, using fallback path');
        SetToGlobalThis("ohosAppDataPath", this.context.filesDir);
      }

      // Load game content after permission is resolved
      windowStage.loadContent(this.defaultPageUri, (err: ESObject, data: ESObject) => {
        if (err.code) {
          TuanjieLog.error('Failed to load the content. Cause: %{public}s', JSON.stringify(err) ?? '');
          return;
        }
        TuanjieLog.info('Succeeded in loading the content. Data: %{public}s', JSON.stringify(data) ?? '');
      });
      this.tuanjiePlayer.onWindowStageCreate(windowStage);
    });
  }

  onWindowStageWillDestroy(): void {
    this.tuanjiePlayer.onWindowStageWillDestroy();
  }
  
  onWindowStageDestroy(): void {
    this.tuanjiePlayer.onWindowStageDestroy()
  }

  onForeground(): void {
    this.tuanjiePlayer.onForeground()
  }

  onBackground(): void {
    this.tuanjiePlayer.onBackground()
  }

  onNewWant(want: Want, launchParam: AbilityConstant.LaunchParam): void {
    this.tuanjiePlayer.onNewWant(want,launchParam)
  }

  setConfig(): void {
    SetToGlobalThis("staticSplashScreenFit", $r('app.integer.StaticSplashScreenFit'));
    SetToGlobalThis("appSplash", $r('app.media.app_splash'));
    SetToGlobalThis("showStaticSplash",$r('app.integer.ShowStaticSplashScreen'));
  }

  private async requestDownloadPermission(): Promise<boolean> {
    const atManager = abilityAccessCtrl.createAtManager();
    const context = this.context as common.UIAbilityContext;

    try {
      const grantStatus = atManager.verifyAccessTokenSync(
        context.applicationInfo.accessTokenId,
        DOWNLOAD_PERMISSION
      );

      if (grantStatus === abilityAccessCtrl.GrantStatus.PERMISSION_GRANTED) {
        TuanjieLog.info('Download directory permission already granted');
        return true;
      }

      TuanjieLog.info('Requesting download directory permission from user...');
      const result = await atManager.requestPermissionsFromUser(context, [DOWNLOAD_PERMISSION]);
      const granted = result.authResults[0] === 0; // 0 = PERMISSION_GRANTED
      TuanjieLog.info(`Permission request result: ${granted ? 'granted' : 'denied'}`);
      return granted;
    } catch (error) {
      const err: BusinessError = error as BusinessError;
      TuanjieLog.error(`Permission request failed. Code: ${err.code}, message: ${err.message}`);
      return false;
    }
  }

  private setupAppDataDir(): void {
    try {
      const downloadRoot = Environment.getUserDownloadDir();
      TuanjieLog.info(`System Download dir: ${downloadRoot}`);
      const appDataDir = `${downloadRoot}/${APP_DATA_SUBDIR}`;
      SetToGlobalThis("ohosAppDataPath", appDataDir);
      if (!fs.accessSync(appDataDir)) {
        fs.mkdirSync(appDataDir, true);
        TuanjieLog.info(`Created app data directory: ${appDataDir}`);
      }
      const nomediaPath = `${appDataDir}/.nomedia`;
      if (!fs.accessSync(nomediaPath)) {
        const file = fs.openSync(nomediaPath, fs.OpenMode.READ_WRITE | fs.OpenMode.CREATE);
        fs.closeSync(file);
        TuanjieLog.info('Created .nomedia flag file');
      }

      TuanjieLog.info(`App data path set to: ${appDataDir}`);
    } catch (error) {
      const err: BusinessError = error as BusinessError;
      TuanjieLog.error(`Failed to setup app data directory. Code: ${err.code}, message: ${err.message}`);
    }
  }

  private registerHmsAbility() {
    SetToGlobalThis("isLaunchSnapshotEnabled", this.isLaunchSnapshotEnabled);
  }

  private isLaunchSnapshotEnabled(): boolean {
    let enable = false;
    if (canIUse("SystemCapability.GraphicsGame.LaunchAcceleration")) {
      //enable = launchAcceleration.isLaunchMirrorEnabled();
      TuanjieLog.info(`launchAcceleration isLaunchMirrorEnabled enable = ${enable}`);
    }
    return enable;
  }
}
