declare namespace BootstrapAlert {

	function info( options: ConfigOptions ): void;
	function alert( options: ConfigOptions ): void;
	function warning( options: ConfigOptions ): void;
	function success( options: ConfigOptions ): void;

	interface ConfigOptions {
		autoHide?: boolean,
		hideTimeout?: number,
		dissmissible?: boolean,
		parentClass?: string,
		innerClass?: string,
		title: string,
		message: string
	}
}